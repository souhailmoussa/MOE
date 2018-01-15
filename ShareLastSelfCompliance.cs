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
    public class ShareLastSelfCompliance : CodeActivity
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

                //retrieve the institute.
                Entity educationalInstitute = service.Retrieve("account", workOrder.GetAttributeValue<EntityReference>("msdyn_serviceaccount").Id, new ColumnSet(true));

                //Get the Self-Compliance Work Order Type.
                string fetch = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='msdyn_workordertype'>
    <attribute name='msdyn_workordertypeid' />
    <attribute name='msdyn_name' />
    <attribute name='createdon' />
    <attribute name='msdyn_taxable' />
    <attribute name='msdyn_incidentrequired' />
    <order attribute='msdyn_name' descending='false' />
    <filter type='and'>
      <condition attribute='net_code' operator='eq' value='004' />
    </filter>
  </entity>
</fetch>";
                EntityCollection workOrderTypes = service.RetrieveMultiple(new FetchExpression(fetch));
                Entity workOrderType = workOrderTypes[0];

                //Retrive all Self-Compliance and order them by created on.
                string selfCompliance = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='msdyn_workorder'>
    <attribute name='msdyn_name' />
    <attribute name='createdon' />
    <attribute name='msdyn_serviceaccount' />
    <attribute name='msdyn_workorderid' />
    <order attribute='createdon' descending='true' />
    <filter type='and'>
      <condition attribute='msdyn_workordertype' operator='eq' value='"+workOrderType.Id+@"' />
    </filter>
  </entity>
</fetch>";
                EntityCollection workOrders = service.RetrieveMultiple(new FetchExpression(selfCompliance));
                Entity workorder = workOrders[0];

                ShareRecordasReadOnly(Resource.GetAttributeValue<EntityReference>("userid").Id, workorder, service);
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
    }
}
