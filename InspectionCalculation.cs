using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Activities;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace MOE.CustomActivity
{
    public class InspectionCalculation : CodeActivity
    {
        /// <summary>
        /// Workflow Input
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

            //Check the InArgument value.
            if(this.Arg_WO != null)
            {
                //Get the Work Order Record.
                Entity workOrder = service.Retrieve("msdyn_workorder", this.Arg_WO.Get<EntityReference>(activityContext).Id, new ColumnSet(true));

                //Retrieve the standards collection for this work order.
                EntityCollection standards = getWorkOrderStandards(service, workOrder.Id);

                //Retieve the elements collection for this work order.
                EntityCollection elements = getWorkOrderElements(service, workOrder.Id);

                if(standards != null)
                {
                    //Retrieve the elements in each standard
                    foreach(Entity standard in standards.Entities)
                    {
                        //define the numebr of applicable element.
                        int totalNumberofApplicableElement = 0;

                        //define the sum of judgment.
                        int sumOfJudgment = 0;

                        //define the weight.
                        int sumOfWeight = 0;

                        //define the Overall Judgment Percentage
                        Decimal overallJudgmentPercentage = 0;

                        //define the weighted score.
                        Decimal weightedScore = 0;

                        //define the weighted score percentage.
                        Decimal weightedScorePercentage = 0;

                        //fetch all elements in the standard
                        EntityCollection elementStandards = getStandardElements(service, standard.Id);

                        foreach(Entity element in elementStandards.Entities)
                        {
                            //Get the compliance status
                            OptionSetValue complianceStatus = element.GetAttributeValue<OptionSetValue>("net_compliantcoordinator");

                            if(complianceStatus != null)
                            {
                                int elementStatus = complianceStatus.Value;
                                //if status != N/A
                                if (elementStatus != 3)
                                {
                                    //increment the total number of applicable element.
                                    totalNumberofApplicableElement = totalNumberofApplicableElement + 1;
                                }
                            }
                            

                            int elementJudgment = element.GetAttributeValue<int>("net_inspectionjudgement");
                            //add the element judgment to the sum of judgment.
                            sumOfJudgment = sumOfJudgment + elementJudgment;

                            //get the weight.
                            int weight = element.GetAttributeValue<int>("net_weight");
                            //add the weight to the sumofweight.
                            sumOfWeight = sumOfWeight + weight;

                            weightedScore = weightedScore + (elementJudgment * weight);
                        }
                        if(totalNumberofApplicableElement != 0)
                        {
                            overallJudgmentPercentage = (Decimal)(sumOfJudgment) * (Decimal) 100 / (Decimal)(2 * totalNumberofApplicableElement);
                        }
                        else
                        {
                            overallJudgmentPercentage = 0;
                        }
                        if(sumOfWeight != 0)
                        {
                            weightedScore = weightedScore / sumOfWeight;
                        }
                        else
                        {
                            weightedScore = 0;
                        }
                        

                        weightedScorePercentage = weightedScore / 2 * 100;

                        standard["net_totalofapplicableelement"] = totalNumberofApplicableElement;
                        standard["net_sumofjudgment"] = sumOfJudgment;
                        standard["net_compliancepercentage"] = overallJudgmentPercentage;
                        standard["net_weightedscore"] = weightedScore;
                        standard["net_weightedscorepercentage"] = weightedScorePercentage;
                        service.Update(standard);
                    }
                }
            }
        }

        private static EntityCollection getWorkOrderStandards(IOrganizationService service, Guid Id)
        {
            string fetch = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='net_inspectionstandard'>
    <attribute name='net_inspectionstandardid' />
    <attribute name='net_name' />
    <attribute name='createdon' />
    <attribute name='net_weightedscorepercentage' />
    <attribute name='net_weightedscore' />
    <attribute name='net_totalofapplicableelement' />
    <attribute name='net_sumofjudgment' />
    <attribute name='net_compliancepercentage' />
    <order attribute='net_name' descending='false' />
    <filter type='and'>
      <condition attribute='net_workorder' operator='eq' uitype='msdyn_workorder' value='" + Id +@"' />
    </filter>
  </entity>
</fetch>";
            EntityCollection standards = service.RetrieveMultiple(new FetchExpression(fetch));
            return standards;
        }

        private static EntityCollection getWorkOrderElements(IOrganizationService service, Guid Id)
        {
            string fetch = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='msdyn_workorderservicetask'>
    <attribute name='createdon' />
    <attribute name='msdyn_workorderservicetaskid' />
    <attribute name='msdyn_name' />
    <attribute name='net_weight' />
    <attribute name='net_inspectionjudgement' />
    <attribute name='net_inspectioncritical' />
    <attribute name='net_compliantcoordinator' />
    <order attribute='createdon' descending='false' />
    <filter type='and'>
      <condition attribute='msdyn_workorder' operator='eq' uitype='msdyn_workorder' value='" + Id + @"' />
    </filter>
  </entity>
</fetch>";
            EntityCollection elements = service.RetrieveMultiple(new FetchExpression(fetch));
            return elements;
        }

        private static EntityCollection getStandardElements(IOrganizationService service, Guid Id)
        {
            //fetch all elements in the standard
            string elementFetch = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='msdyn_workorderservicetask'>
    <attribute name='createdon' />
    <attribute name='msdyn_workorderservicetaskid' />
    <attribute name='msdyn_name' />
    <attribute name='net_weight' />
    <attribute name='net_inspectionjudgement' />
    <attribute name='net_inspectioncritical' />
    <attribute name='net_compliantcoordinator' />
    <order attribute='createdon' descending='false' />
    <link-entity name='msdyn_workorderservice' from='msdyn_workorderserviceid' to='net_service' alias='aj'>
      <link-entity name='net_inspectionstandard' from='net_inspectionstandardid' to='net_inspectionstandard' alias='ak'>
        <filter type='and'>
          <condition attribute='net_inspectionstandardid' operator='eq' uitype='net_inspectionstandard' value='" + Id+@"' />
        </filter>
      </link-entity>
    </link-entity>
  </entity>
</fetch>";
            EntityCollection elementsOfStandard = service.RetrieveMultiple(new FetchExpression(elementFetch));
            return elementsOfStandard;
        }
    }
}
