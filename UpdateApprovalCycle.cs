using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Xrm.Sdk.Client;
using System.Activities;

namespace MOE.CustomActivity
{
    public class UpdateApprovalCycle : CodeActivity
    {
        /// <summary>
        /// Workflow input
        /// </summary>
        [RequiredArgument]
        [Input("Approval Cycle")]
        [ReferenceTarget("net_approvalcycle_wo")]
        public InArgument<EntityReference> Arg_AC
        {
            get;
            set;
        }
        /// <summary>
        /// Workflow input
        /// </summary>
        [RequiredArgument]
        [Input("Work Order")]
        [ReferenceTarget("msdyn_workorder")]
        public InArgument<EntityReference> Arg_WO
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
            if (this.Arg_WO != null)
            {
                //Get the approval cycle record.
                Entity approvalCycle = (Entity)service.Retrieve("net_approvalcycle_wo", this.Arg_AC.Get<EntityReference>(activityContext).Id, new ColumnSet(true));

                //Get the Work Order record.
                Entity entity = (Entity)service.Retrieve("msdyn_workorder", this.Arg_WO.Get<EntityReference>(activityContext).Id, new ColumnSet(true));

                //Retrieve the Educational Institute.
                Entity educationalInstitute = (Entity)service.Retrieve("account", entity.GetAttributeValue<EntityReference>("msdyn_serviceaccount").Id, new ColumnSet(true));

                //Retrieve the Educational Institute Type.
                Entity instituteType = (Entity)service.Retrieve("net_educationalinstitutetype", 
                    educationalInstitute.GetAttributeValue<EntityReference>("net_typeofinstitute").Id, new ColumnSet(true));

                //Retrieve the first QA Configuration record.
                string fetch = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='net_qaconfiguration'>
    <attribute name='net_qaconfigurationid' />
    <attribute name='net_name' />
    <attribute name='createdon' />
    <attribute name='net_numberofdays' />
    <order attribute='net_name' descending='false' />
    <filter type='and'>
      <condition attribute='statecode' operator='eq' value='0' />
    </filter>
  </entity>
</fetch>";
                EntityCollection QAConfigurations = service.RetrieveMultiple(new FetchExpression(fetch));
                Entity QAConfiguration = QAConfigurations.Entities.FirstOrDefault<Entity>();
                int numberOfDaysToWait = QAConfiguration.GetAttributeValue<int>("net_numberofdays");

                //Get the school id.
                string schoolSIS = educationalInstitute.GetAttributeValue<string>("net_schoolid");
                //get the year.
                DateTime now = DateTime.Now;
                string year = now.Year.ToString();
                //get the visit type.
                Entity visitType = (Entity)service.Retrieve("net_inspectionvisittype", entity.GetAttributeValue<EntityReference>("net_inspectionvisittype").Id, new ColumnSet(true));
                string nameOfVisit = visitType.GetAttributeValue<string>("net_name");
                //get the work order number.
                string workOrderNumber = entity.GetAttributeValue<string>("msdyn_name");
                //get the term.
                string term = entity.FormattedValues["net_term"];

                string name = schoolSIS + "/" + year + "/" + term + "/" + nameOfVisit + "/" + workOrderNumber;

                //Update the Approval Cycle Record.
                approvalCycle["net_institutetype"] = new EntityReference("net_educationalinstitutetype", instituteType.Id);
                approvalCycle["net_qaresponsedaysnumber"] = numberOfDaysToWait;
                approvalCycle["net_name"] = name;
                service.Update(approvalCycle);
            }
        }
    }
}
