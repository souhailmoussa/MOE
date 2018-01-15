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
    public class UpdateDomainName : CodeActivity
    {
        /// <summary>
        /// Workflow input
        /// </summary>
        [RequiredArgument]
        [Input("Domain")]
        [ReferenceTarget("net_substandard")]
        public InArgument<EntityReference> Arg_WO
        {
            get;
            set;
        }

        protected override void Execute(CodeActivityContext activityContext)
        {
            //Create the tracing service.
            ITracingService tracingService = activityContext.GetExtension<ITracingService>();

            //create the context.
            IWorkflowContext workflowContext = activityContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = activityContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(workflowContext.UserId);

            //check the InArgument Value.

            if (this.Arg_WO != null)
            {
                Entity entity = (Entity)service.Retrieve("net_substandard", this.Arg_WO.Get<EntityReference>(activityContext).Id, new ColumnSet(true));
                string domainName = (string)entity["net_name"];
                if(domainName.EndsWith(" s"))
                {
                    domainName = domainName.Substring(0, (domainName.Length - 2));
                    entity["net_name"] = domainName;
                    service.Update(entity);
                }
            }
        }
    }
}
