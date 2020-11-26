using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System.Activities;
using Microsoft.Crm.Sdk.Messages;

namespace MOE.CustomActivity
{
    public class SetSectionHeadInResource : CodeActivity
    {
        /// <summary>
        /// Workflow input
        /// </summary>
        [RequiredArgument]
        [Input("Bookable Resource")]
        [ReferenceTarget("bookableresource")]
        public InArgument<EntityReference> Arg_BR
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

            if(this.Arg_BR != null)
            {
                //Get the bookable resource record.
                Entity entity = (Entity)service.Retrieve("bookableresource", this.Arg_BR.Get<EntityReference>(activityContext).Id, new ColumnSet(true));

                //Get the emirate of the resource.
                Entity emirate = service.Retrieve("net_emirate", entity.GetAttributeValue<EntityReference>("net_emirate").Id, new ColumnSet(true));

                //Get the emirate section head
                EntityReference sectionHead = emirate.GetAttributeValue<EntityReference>("net_sectionhead");

                //set the emirate section head in the resource record.
                entity["net_sectionhead"] = new EntityReference("systemuser", sectionHead.Id);
                //Update the resource record.
                service.Update(entity);

                // Create the Request Object and Set the Request Object's Properties
                AssignRequest assign = new AssignRequest
                {
                    Assignee = new EntityReference("systemuser", sectionHead.Id),
                    Target = new EntityReference("bookableresource", entity.Id)
                };


                // Execute the Request
                service.Execute(assign);

            }
        }
    }
}
