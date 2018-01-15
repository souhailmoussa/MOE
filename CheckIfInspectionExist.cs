using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System.Activities;

namespace MOE.CustomActivity
{
    public class CheckIfInspectionExist : CodeActivity
    {
        /// <summary>
        /// Workflow input
        /// </summary>
        [RequiredArgument]
        [Input("Educational Institute")]
        [ReferenceTarget("account")]
        public InArgument<EntityReference> Arg_Account
        {
            get;
            set;
        }
        /// <summary>
        /// Workflow input
        /// </summary>
        [RequiredArgument]
        [Input("Inspection")]
        [ReferenceTarget("msdyn_workordertype")]
        public InArgument<EntityReference> Arg_WOT
        {
            get;
            set;
        }
        /// <summary>
        /// Workflow input
        /// </summary>
        [RequiredArgument]
        [Input("QA Visit")]
        [ReferenceTarget("net_inspectionvisittype")]
        public InArgument<EntityReference> Arg_QAVisit
        {
            get;
            set;
        }
        protected override void Execute(CodeActivityContext activityContext)
        {
            //Create the tracing service.
            ITracingService tracingService = activityContext.GetExtension<ITracingService>();

            //Create the context.
            IWorkflowContext workflowContext = activityContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = activityContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(workflowContext.UserId);

            //Check the InArgument value
            if (this.Arg_Account != null && this.Arg_WOT != null && this.Arg_QAVisit != null)
            {
                //Get the Educational Institute record.
                Entity educationalInstitute = (Entity)service.Retrieve("account", this.Arg_Account.Get<EntityReference>(activityContext).Id, new ColumnSet(true));

                //Get the Work Order Type.
                Entity workOrderType = (Entity)service.Retrieve("msdyn_workordertype", this.Arg_WOT.Get<EntityReference>(activityContext).Id, new ColumnSet(true));

                //Get the QA visit.
                Entity QAVisit = (Entity)service.Retrieve("net_inspectionvisittype", this.Arg_QAVisit.Get<EntityReference>(activityContext).Id, new ColumnSet(true));

                //Retrieve the start date and end date of the educational institute.
                DateTime startDate = educationalInstitute.GetAttributeValue<DateTime>("net_startdate");
                DateTime endDate = educationalInstitute.GetAttributeValue<DateTime>("net_enddate");

                //Retrieve a collection of active inspection and is not a QA visit that is done on that institute.
                string fetch = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='msdyn_workorder'>
    <attribute name='msdyn_name' />
    <attribute name='createdon' />
    <attribute name='msdyn_datewindowend' />
    <attribute name='msdyn_datewindowstart' />
    <attribute name='msdyn_serviceaccount' />
    <attribute name='msdyn_workorderid' />
    <order attribute='msdyn_name' descending='false' />
    <filter type='and'>
      <condition attribute='msdyn_serviceaccount' operator='eq' value='" + educationalInstitute.Id + @"' />
      <condition attribute='msdyn_workordertype' operator='eq' value='" + workOrderType.Id + @"' />
      <condition attribute='statecode' operator='eq' value='0' />
      <condition attribute='net_inspectionvisittype' operator='ne' uitype='net_inspectionvisittype' value='" + QAVisit.Id + @"' />
    </filter>
  </entity>
</fetch>";
                EntityCollection workOrders = service.RetrieveMultiple(new FetchExpression(fetch));
                if (workOrders != null)
                {
                    //Define a global integer.
                    int numberOfRecords = 0;

                    //check for each inspection if the start date of the Educational Insitute is in the range of the work order
                    foreach (Entity workOrder in workOrders.Entities)
                    {
                        DateTime workOrderStartDate = workOrder.GetAttributeValue<DateTime>("msdyn_datewindowstart");
                        DateTime workOrderEndDate = workOrder.GetAttributeValue<DateTime>("msdyn_datewindowend");
                        if (startDate >= workOrderStartDate && startDate <= workOrderEndDate)
                        {
                            numberOfRecords = numberOfRecords + 1;
                        }
                    }
                    //if there is an active inspection on that institute in the range specified, fire an exception.
                    if (numberOfRecords > 0)
                    {
                        throw new InvalidPluginExecutionException("There is an active inspection in the range specified. please specify another start and end date.");
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }
    }
}
