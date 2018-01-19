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
using Microsoft.Crm.Sdk.Messages;

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

                //Retrieve the domains collection for this work order.
                EntityCollection domains = getWorkOrderDomains(service, workOrder.Id);

                //Retieve the elements collection for this work order.
                EntityCollection elements = getWorkOrderElements(service, workOrder.Id);

                #region Domain Calculation
                if(domains != null)
                {
                    //Retrieve each domain and update it.
                    foreach(Entity domain in domains.Entities)
                    {
                        Entity domainRecord = service.Retrieve("msdyn_workorderservice", domain.Id, new ColumnSet(true));
                        CalculateRollupFieldRequest totalApplicableElementRequest =

new CalculateRollupFieldRequest { Target = new EntityReference("msdyn_workorderservice", domainRecord.Id), FieldName = "net_totalnumberofapplicableelements" };

                        CalculateRollupFieldResponse totalApplicableElementResponse = (CalculateRollupFieldResponse)service.Execute(totalApplicableElementRequest);

                        domainRecord = totalApplicableElementResponse.Entity;

                        //service.Update(workOrder);

                        CalculateRollupFieldRequest SumOfJudgmentRequest =

        new CalculateRollupFieldRequest { Target = new EntityReference("msdyn_workorderservice", domainRecord.Id), FieldName = "net_sumofjudgment" };

                        CalculateRollupFieldResponse sumOfJudgmentResponse = (CalculateRollupFieldResponse)service.Execute(SumOfJudgmentRequest);

                        domainRecord = sumOfJudgmentResponse.Entity;

                        service.Update(domainRecord);

                        Entity domainUpdated = service.Retrieve("msdyn_workorderservice", domainRecord.Id, new ColumnSet(true));
                        int totalApplicableELement = domainUpdated.GetAttributeValue<int>("net_totalnumberofapplicableelements");
                        int sumOfJudgement = domainUpdated.GetAttributeValue<int>("net_sumofjudgment");
                        int AgreePercentage = 0;
                        if (totalApplicableELement != 0)
                        {
                            AgreePercentage = sumOfJudgement * 100 / (totalApplicableELement * 2);
                        }
                        else
                        {
                            AgreePercentage = 0;
                        }

                        domainUpdated["net_agreepercentage"] = AgreePercentage;
                        service.Update(domainUpdated);
                    }
                }
                #endregion

                #region Standard Calculation
                if (standards != null)
                {
                    //Retrieve the elements in each standard
                    foreach (Entity standard in standards.Entities)
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

                        foreach (Entity element in elementStandards.Entities)
                        {
                            //Get the compliance status
                            OptionSetValue complianceStatus = element.GetAttributeValue<OptionSetValue>("net_compliantcoordinator");

                            if (complianceStatus != null)
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
                        if (totalNumberofApplicableElement != 0)
                        {
                            overallJudgmentPercentage = (Decimal)(sumOfJudgment) * (Decimal)100 / (Decimal)(2 * totalNumberofApplicableElement);
                        }
                        else
                        {
                            overallJudgmentPercentage = 0;
                        }
                        if (sumOfWeight != 0)
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
                #endregion

                #region Update the Rollup Fields
                CalculateRollupFieldRequest rollupRequest =

new CalculateRollupFieldRequest { Target = new EntityReference("msdyn_workorder", workOrder.Id), FieldName = "net_totalnumberofapplicableelements" };

                CalculateRollupFieldResponse response = (CalculateRollupFieldResponse)service.Execute(rollupRequest);

                workOrder = response.Entity;

                //service.Update(workOrder);

                CalculateRollupFieldRequest rollupRequestSumOfJudgment =

new CalculateRollupFieldRequest { Target = new EntityReference("msdyn_workorder", workOrder.Id), FieldName = "net_sumofjudgment" };

                CalculateRollupFieldResponse responseSumOfJudgment = (CalculateRollupFieldResponse)service.Execute(rollupRequestSumOfJudgment);

                workOrder = responseSumOfJudgment.Entity;

                service.Update(workOrder);
                #endregion

                #region Work Order Calculation
                Entity updatedWorkOrder = service.Retrieve("msdyn_workorder", workOrder.Id, new ColumnSet(true));

                int totalOfApplicableElements = updatedWorkOrder.GetAttributeValue<int>("net_totalnumberofapplicableelements");
                int workOrderSumOfJudgment = updatedWorkOrder.GetAttributeValue<int>("net_sumofjudgment");
                //int maxPossibleScore = workOrder.GetAttributeValue<int>("net_maxpossiblescore");
                int maxPossibleScore = totalOfApplicableElements * 2;
                Decimal overallCompliancePercentage = 0;
                if (maxPossibleScore != 0)
                {
                    overallCompliancePercentage = (Decimal)((Decimal)workOrderSumOfJudgment * 100 / (Decimal)maxPossibleScore);
                }
                else
                {
                    overallCompliancePercentage = 0;
                }

                //define the weighted score.
                Decimal workOrderWeightedScore = 0;

                //define the weight.
                int workOrderSumOfWeight = 0;

                if (elements != null)
                {
                    string elementsID = "";
                    Decimal RiskLevelPercentage = 0;
                    foreach (Entity element in elements.Entities)
                    {
                        elementsID = (elementsID + "<value>" + element.Id + "</value>");

                        int elementJudgment = element.GetAttributeValue<int>("net_inspectionjudgement");

                        //get the weight.
                        int weight = element.GetAttributeValue<int>("net_weight");
                        //add the weight to the sumofweight.
                        workOrderSumOfWeight = workOrderSumOfWeight + weight;

                        workOrderWeightedScore = workOrderWeightedScore + (elementJudgment * weight);
                    }

                    if (workOrderSumOfWeight != 0)
                    {
                        workOrderWeightedScore = workOrderWeightedScore / workOrderSumOfWeight;
                    }
                    else
                    {
                        workOrderWeightedScore = 0;
                    }

                    //retrieve the number of critical element definition.
                    //EntityCollection criticalElementDefinition = getCriticalElementsDefinitionByElementInstance(service, elementsID);
                    //int numberOfCriticalElementsDefinition = criticalElementDefinition.Entities.Count;

                    string fetch = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
  <entity name='net_element'>
    <attribute name='net_elementid' />
    <attribute name='net_name' />
    <attribute name='createdon' />
    <order attribute='net_name' descending='false' />
    <filter type='and'>
      <condition attribute='net_critical' operator='eq' value='1' />
    </filter>
    <link-entity name='msdyn_workorderservicetask' from='net_element' to='net_elementid' alias='ab'>
      <filter type='and'>
        <condition attribute='msdyn_workorderservicetaskid' operator='in'>"
        + elementsID +
        "</condition>" +
        "</filter>" +
        "</link-entity>" +
        "</entity>" +
        "</fetch>";
                    EntityCollection criticalElementsDefinition = service.RetrieveMultiple(new FetchExpression(fetch));
                    int numberOfCriticalElementsDefinition = criticalElementsDefinition.Entities.Count;

                    //Retrieve the number of critical elements.
                    int numberOfCriticalElements = getWorkOrderCriticalElements(service, workOrder.Id).Entities.Count;

                    if (numberOfCriticalElementsDefinition == 0)
                    {
                        RiskLevelPercentage = 0;
                    }
                    else
                    {
                        RiskLevelPercentage = (Decimal)numberOfCriticalElements * (Decimal)100 / (Decimal)numberOfCriticalElementsDefinition;
                    }

                    //update the work order.
                    updatedWorkOrder["net_overallcompliance"] = overallCompliancePercentage;
                    updatedWorkOrder["net_weightedscore"] = workOrderWeightedScore;
                    updatedWorkOrder["net_numberofcriticalinspectionelements"] = numberOfCriticalElements;
                    updatedWorkOrder["net_numberofcriticalelementdefinition"] = numberOfCriticalElementsDefinition;
                    updatedWorkOrder["net_riskslevelpercentage"] = RiskLevelPercentage;

                    service.Update(updatedWorkOrder);
                }
                #endregion

            }
        }

        //Retrieve list of inspection standards.
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

        //Retrieve list inspection elements of the work order.
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

        //Retrieve list of elements related to specific standard.
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

        //Retrieve list of critical element of the work order.
        private static EntityCollection getWorkOrderCriticalElements(IOrganizationService service, Guid WorkOrderId)
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
              <condition attribute='msdyn_workorder' operator='eq' uitype='msdyn_workorder' value='" + WorkOrderId + @"' />
              <condition attribute='net_inspectioncritical' operator='eq' value='1' />
            </filter>
          </entity>
        </fetch>";
            EntityCollection elements = service.RetrieveMultiple(new FetchExpression(fetch));
            return elements;
        }

        //Retrieve list of critical element definition of a inspection element list.
        private static EntityCollection getCriticalElementsDefinitionByElementInstance(IOrganizationService service, string elementsId)
        {
            string fetch = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
  <entity name='net_element'>
    <attribute name='net_elementid' />
    <attribute name='net_name' />
    <attribute name='createdon' />
    <order attribute='net_name' descending='false' />
    <filter type='and'>
      <condition attribute='net_critical' operator='eq' value='1' />
    </filter>
    <link-entity name='msdyn_workorderservicetask' from='net_element' to='net_elementid' alias='ab'>
      <filter type='and'>
        <condition attribute='msdyn_workorderservicetaskid' operator='in' value='" + elementsId +@"' />
      </filter>
    </link-entity>
  </entity>
</fetch>";
            EntityCollection elementDefinition = service.RetrieveMultiple(new FetchExpression(fetch));
            return elementDefinition;
        }

        private static EntityCollection getWorkOrderDomains(IOrganizationService service, Guid workOrderId)
        {
            string fetch = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='msdyn_workorderservice'>
    <attribute name='createdon' />
    <attribute name='msdyn_workorder' />
    <attribute name='msdyn_name' />
    <attribute name='msdyn_workorderserviceid' />
    <attribute name='net_totalnumberofapplicableelements' />
    <attribute name='net_sumofjudgment' />
    <attribute name='net_agreepercentage' />
    <order attribute='msdyn_name' descending='false' />
    <filter type='and'>
      <condition attribute='msdyn_workorder' operator='eq' uitype='msdyn_workorder' value='" + workOrderId + @"' />
    </filter>
  </entity>
</fetch>";
            EntityCollection domains = service.RetrieveMultiple(new FetchExpression(fetch));
            return domains;
        }
    }
}
