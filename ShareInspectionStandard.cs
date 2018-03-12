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
    public class ShareInspectionStandard : CodeActivity
    {
        /// <summary>
        /// Workflow input
        /// </summary>
        [RequiredArgument]
        [Input("Bookable Resource Booking")]
        [ReferenceTarget("bookableresourcebooking")]
        public InArgument<EntityReference> Arg_BRB
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
            if (this.Arg_BRB != null)
            {
                //Get the Bookable Resource Booking record.
                Entity entity = (Entity)service.Retrieve("bookableresourcebooking", this.Arg_BRB.Get<EntityReference>(activityContext).Id, new ColumnSet(true));

                //Get the Bookable Resource
                Entity Resource = service.Retrieve("bookableresource", entity.GetAttributeValue<EntityReference>("resource").Id, new ColumnSet(true));

                //retreieve the work order booked on.
                Entity workOrder = service.Retrieve("msdyn_workorder", entity.GetAttributeValue<EntityReference>("msdyn_workorder").Id, new ColumnSet(true));

                //retrieve if resource is team member or team leader.
                int inspectorRole = entity.GetAttributeValue<OptionSetValue>("net_role").Value;

                string fetch = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='net_inspectionstandard'>
    <attribute name='net_inspectionstandardid' />
    <attribute name='net_name' />
    <attribute name='createdon' />
    <order attribute='net_name' descending='false' />
    <filter type='and'>
      <condition attribute='net_workorder' operator='eq' uitype='msdyn_workorder' value='"+workOrder.Id+@"' />
    </filter>
  </entity>
</fetch>";
                EntityCollection inspectionStandards = service.RetrieveMultiple(new FetchExpression(fetch));

                if(inspectionStandards != null)
                {
                    if(inspectionStandards.Entities.Count > 0)
                    {
                        //If Team Member.
                        if(inspectorRole == 1)
                        {
                            foreach (Entity inspectionStandard in inspectionStandards.Entities)
                            {
                                ShareRecordasReadOnly(Resource.GetAttributeValue<EntityReference>("userid").Id, inspectionStandard, service);
                            }
                        }
                        //If Team Leader.
                        else if(inspectorRole == 2)
                        {
                            foreach (Entity inspectionStandard in inspectionStandards.Entities)
                            {
                                ShareRecord(Resource.GetAttributeValue<EntityReference>("userid").Id, inspectionStandard, service);
                            }
                        }
                    }
                }
            }
        }

        //Share function
        private void ShareRecordasReadOnly(Guid Userid, Entity task, IOrganizationService service)
        {
            GrantAccessRequest grantRequest = new GrantAccessRequest()
            {

                Target = new EntityReference(task.LogicalName, task.Id),
                PrincipalAccess = new PrincipalAccess()
                {
                    Principal = new EntityReference("systemuser", Userid),
                    AccessMask = AccessRights.ReadAccess
                }
            };

            // Execute the request.
            GrantAccessResponse grantResponse =
                (GrantAccessResponse)service.Execute(grantRequest);
        }

        //Share function
        private void ShareRecord(Guid Userid, Entity task, IOrganizationService service)
        {
            GrantAccessRequest grantRequest = new GrantAccessRequest()
            {

                Target = new EntityReference(task.LogicalName, task.Id),
                PrincipalAccess = new PrincipalAccess()
                {
                    Principal = new EntityReference("systemuser", Userid),
                    AccessMask = AccessRights.ReadAccess | AccessRights.WriteAccess | AccessRights.ShareAccess | AccessRights.AssignAccess | AccessRights.AppendToAccess | AccessRights.AppendAccess
                }
            };

            // Execute the request.
            GrantAccessResponse grantResponse =
                (GrantAccessResponse)service.Execute(grantRequest);
        }
    }
}
