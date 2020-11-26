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
  public  class MovingAnnotations : CodeActivity
    {


        /// <summary>
        /// Workflow input
        /// </summary>
        [RequiredArgument]
        [Input("Aproval Cycle")]
        [ReferenceTarget("net_approvalcycle_wo")]
        public InArgument<EntityReference> Arg_Cycle
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
            if (this.Arg_Cycle != null)
            {
                //Get the Team Record.
                Entity approval = (Entity)service.Retrieve("net_approvalcycle_wo", this.Arg_Cycle.Get<EntityReference>(activityContext).Id, new ColumnSet("net_approvalcycle_woid", "net_workorder"));
                Entity WO = service.Retrieve("msdyn_workorder", approval.GetAttributeValue<EntityReference>("net_workorder").Id, new ColumnSet("msdyn_serviceaccount"));
                EntityReference accountid = WO.GetAttributeValue<EntityReference>("msdyn_serviceaccount");
                QueryExpression q = new QueryExpression("annotation");
                q.ColumnSet = new ColumnSet(true);
                q.Criteria.AddCondition(new ConditionExpression("objectid", ConditionOperator.Equal, approval.Id));
                q.AddOrder("createdon", OrderType.Ascending);

                EntityCollection notes = service.RetrieveMultiple(q);
                if (notes != null && notes.Entities.Count > 0)
                {
                    Entity item = service.Retrieve("annotation", notes.Entities.Last().Id, new ColumnSet(true));
                    Entity _Attachment = new Entity("annotation");
                    if (item.Contains("subject"))
                        _Attachment["subject"] = item["subject"];
                    _Attachment["objectid"] = new EntityReference("account", accountid.Id);
                    //_Attachment["objecttypecode"] = "email";
                    if (item.Contains("filename"))
                        _Attachment["filename"] = item["filename"];
                    if (item.Contains("documentbody"))
                        _Attachment["documentbody"] = item["documentbody"];
                    if (item.Contains("mimetype"))
                        _Attachment["mimetype"] = item["mimetype"];
                    service.Create(_Attachment);

                }
            }
        }
    }
}

            