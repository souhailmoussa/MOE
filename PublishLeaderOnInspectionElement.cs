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
    public class PublishLeaderOnInspectionElement : CodeActivity
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

                int teamLeader = entity.GetAttributeValue<OptionSetValue>("net_role").Value;

                //If it is team leader
                if (teamLeader == 2)
                {
                    //Get the Bookable Resource
                    Entity Resource = service.Retrieve("bookableresource", entity.GetAttributeValue<EntityReference>("resource").Id, new ColumnSet(true));

                    //retreieve the work order booked on.
                    Entity workOrder = service.Retrieve("msdyn_workorder", entity.GetAttributeValue<EntityReference>("msdyn_workorder").Id, new ColumnSet(true));

                    //Retrive all Inspection Element related to that work order.
                    string fetch = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='msdyn_workorderservicetask'>
    <attribute name='createdon' />
    <attribute name='msdyn_name' />
    <attribute name='net_teamleader' />
    <attribute name='msdyn_workorderservicetaskid' />
    <order attribute='msdyn_lineorder' descending='false' />
    <filter type='and'>
      <condition attribute='msdyn_workorder' operator='eq' value='" + workOrder.Id + @"' />
    </filter>
  </entity>
</fetch>";

                    EntityCollection inspectionElements = service.RetrieveMultiple(new FetchExpression(fetch));
                    if (inspectionElements != null)
                    {
                        foreach (Entity inspectionElement in inspectionElements.Entities)
                        {
                            Guid userId = Resource.GetAttributeValue<EntityReference>("userid").Id;
                            inspectionElement["net_teamleader"] = new EntityReference("systemuser", userId);
                            service.Update(inspectionElement);
                        }
                    }
                    else
                        return;
                }
                else
                    return;
            }
        }
    }
}
