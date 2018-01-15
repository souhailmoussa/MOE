using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System.Activities;
using Microsoft.Xrm.Sdk.Workflow;

namespace MOE.CustomActivity
{
    public class CheckIfSelfComplianceExist : CodeActivity
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
        [Input("Work Order Type")]
        [ReferenceTarget("msdyn_workordertype")]
        public InArgument<EntityReference> Arg_WOT
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
            if (this.Arg_Account != null && this.Arg_WOT != null)
            {
                //Get the Educational Institute record.
                Entity educationalInstitute = (Entity)service.Retrieve("account", this.Arg_Account.Get<EntityReference>(activityContext).Id, new ColumnSet(true));

                //Get the Work Order Type.
                Entity workOrderType = (Entity)service.Retrieve("msdyn_workordertype", this.Arg_WOT.Get<EntityReference>(activityContext).Id, new ColumnSet(true));

                //Retrieve the start date and end date of the educational institute.
                DateTime startDate = educationalInstitute.GetAttributeValue<DateTime>("net_startdate");
                DateTime endDate = educationalInstitute.GetAttributeValue<DateTime>("net_enddate");

                //Retrieve a collection of active self-compliance that is done on that institute.
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
      <condition attribute='msdyn_serviceaccount' operator='eq' value='" + educationalInstitute.Id+ @"' />
      <condition attribute='msdyn_workordertype' operator='eq' value='" + workOrderType.Id + @"' />
      <condition attribute='statecode' operator='eq' value='0' />
    </filter>
  </entity>
</fetch>";
                EntityCollection workOrders = service.RetrieveMultiple(new FetchExpression(fetch));
                if(workOrders != null)
                {
                    //Define a global integer.
                    int numberOfRecords = 0;

                    //check for each self compliance if the start date of the Educational Insitute is in the range of the work order
                    foreach (Entity workOrder in workOrders.Entities)
                    {
                        DateTime workOrderStartDate = workOrder.GetAttributeValue<DateTime>("msdyn_datewindowstart");
                        DateTime workOrderEndDate = workOrder.GetAttributeValue<DateTime>("msdyn_datewindowend");
                        if (startDate >= workOrderStartDate && startDate <= workOrderEndDate)
                        {
                            numberOfRecords = numberOfRecords + 1;
                        }
                    }
                    //if there is an active self compliance on that institute in the range specified, fire an exception.
                    if(numberOfRecords > 0)
                    {
                        throw new InvalidPluginExecutionException("There is an active self compliance in the range specified. please specify another start and end date.");
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
