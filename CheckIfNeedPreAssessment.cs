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
    public class CheckIfNeedPreAssessment : CodeActivity
    {
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
                //Get the Work Order record.
                Entity entity = (Entity)service.Retrieve("msdyn_workorder", this.Arg_WO.Get<EntityReference>(activityContext).Id, new ColumnSet(true));
                //Get the Work Order Type.
                EntityReference WOType = (EntityReference)entity["msdyn_workordertype"];
                Entity WOTypeEntity = service.Retrieve("msdyn_workordertype", WOType.Id, new ColumnSet(true));

                //check if this type need pre-assessment.
                bool WOTypeNeedPreAssessment = (bool)WOTypeEntity["net_needpreassessment"];

                //Get the Educational Institute
                EntityReference educationalInstitute = (EntityReference)entity["msdyn_serviceaccount"];
                Entity educationalInstituteEntity = service.Retrieve("account", educationalInstitute.Id, new ColumnSet(true));

                EntityReference instituteType = (EntityReference)educationalInstituteEntity["net_typeofinstitute"];
                Entity instituteTypeEntity = service.Retrieve("net_educationalinstitutetype", instituteType.Id, new ColumnSet(true));

                bool InstituteTypeNeedPreAssessment = (bool)instituteTypeEntity["net_needpreassessment"];

                bool WONeedPreAssessment = WOTypeNeedPreAssessment & InstituteTypeNeedPreAssessment;

                entity["net_needpreassessment"] = WONeedPreAssessment;
                service.Update(entity);
            }
        }
    }
}
