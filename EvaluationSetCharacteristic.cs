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
    public class EvaluationSetCharacteristic : CodeActivity
    {
        /// <summary>
        /// Workflow input
        /// </summary>
        [RequiredArgument]
        [Input("Bookable Resource Booking")]
        [ReferenceTarget("bookableresourcebooking")]
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
                //Get the Bookable Resource Booking record.
                Entity entity = (Entity)service.Retrieve("bookableresourcebooking", this.Arg_WO.Get<EntityReference>(activityContext).Id, new ColumnSet(true));

                //Get the resource
                EntityReference Resource = (EntityReference)entity["resource"];

                //Get the bookable resource characteristic
                ConditionExpression conditionExpression = new ConditionExpression();
                conditionExpression.AttributeName = "resource";
                conditionExpression.Values.Add(Resource.Id);

                FilterExpression FilterExpression = new FilterExpression();
                FilterExpression.Conditions.Add(conditionExpression);

                QueryExpression QueryExpression = new QueryExpression("bookableresourcecharacteristic");
                QueryExpression.ColumnSet = new ColumnSet(true);
                QueryExpression.Criteria.AddFilter(FilterExpression);

                EntityCollection bookableResourceCharacteristics = service.RetrieveMultiple(QueryExpression);

                if (bookableResourceCharacteristics != null)
                {
                    Entity bookableResourceCharacteristic = bookableResourceCharacteristics[0];

                    EntityReference CharacteristicEntityReference = (EntityReference)bookableResourceCharacteristic["characteristic"];
                    Entity Characteristic = service.Retrieve("characteristic", CharacteristicEntityReference.Id, new ColumnSet(true));

                    string characteristicName = (string)Characteristic["name"];
                    entity["net_characteristic"] = characteristicName;

                    service.Update(entity);
                }


            }
        }
    }
}
