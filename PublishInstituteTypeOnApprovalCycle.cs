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
    public class PublishInstituteTypeOnApprovalCycle : CodeActivity
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

                //Update the Approval Cycle Record.
                approvalCycle["net_institutetype"] = new EntityReference("net_educationalinstitutetype", instituteType.Id);
                service.Update(approvalCycle);
            }
        }
    }
}
