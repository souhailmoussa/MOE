using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Discovery;
using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Xrm.Sdk.Client;
using System.Activities;
using System.Xml.Linq;

namespace MOE.CustomActivity
{
    public class ServiceTaskAutoCreation : CodeActivity
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

                //Get the Work Order Visit Type.
                EntityReference VisitType = (EntityReference)entity["net_inspectionvisittype"];

                EntityReference educationalInstitute = (EntityReference)entity["msdyn_serviceaccount"];
                CreateServiceTaskFromDatabase(service, entity, educationalInstitute, WOType, VisitType);

                //bool needPreAssessment = (bool)entity["net_needpreassessment"];
                //if (!needPreAssessment)
                //{

                //}
            }
        }

        private static void CreateServiceTaskFromDatabase(IOrganizationService service, Entity entity,
            EntityReference EducationalInstituteReference, EntityReference WorkOrderType, EntityReference VisitType)
        {
            //Retrieve the educational institute record.
            Entity educationalInstitute = service.Retrieve("account", EducationalInstituteReference.Id, new ColumnSet(true));

            //Retrieve a collection of standard related to the institute type.
            EntityCollection standardList = GetRelatedStandards(service, educationalInstitute, WorkOrderType, VisitType);

            EntityCollection subStandardList = GetRelatedSubStandards(service, standardList, entity);

            EntityCollection elementList = GetRelatedElements(service, subStandardList, entity);

            foreach (Entity Element in elementList.Entities)
            {
                //Create a new Service Task Entity
                Entity ServiceTask = new Entity("msdyn_workorderservicetask");

                string Name = (string)Element["net_name"];
                EntityReference SubStandard = (EntityReference)Element["net_substandard"];
                bool Critical = (bool)Element["net_critical"];
                //string byLaw = (string)Element["net_bylaw"];

                string byLaw = Element.GetAttributeValue<string>("net_bylaw");
                

                Entity SubStandardEntity = service.Retrieve("net_substandard", SubStandard.Id, new ColumnSet(true));

                EntityReference Standard = (EntityReference)SubStandardEntity["net_standard"];

                //Retrieve the work order service related to the sub-standard in this inspection.
                ConditionExpression conditionExpression = new ConditionExpression();
                conditionExpression.AttributeName = "net_substandard";
                conditionExpression.Operator = ConditionOperator.Equal;
                conditionExpression.Values.Add(SubStandard.Id);

                ConditionExpression workOrderConditionExpression = new ConditionExpression();
                workOrderConditionExpression.AttributeName = "msdyn_workorder";
                workOrderConditionExpression.Operator = ConditionOperator.Equal;
                workOrderConditionExpression.Values.Add(entity.Id);



                FilterExpression filterExpression = new FilterExpression();
                filterExpression.FilterOperator = LogicalOperator.And;
                filterExpression.Conditions.Add(conditionExpression);
                filterExpression.Conditions.Add(workOrderConditionExpression);

                QueryExpression queryExpression = new QueryExpression("msdyn_workorderservice");
                queryExpression.ColumnSet = new ColumnSet(true);
                queryExpression.Criteria.AddFilter(filterExpression);

                //retrieve a collection of services related to that sub standard in this work order.
                EntityCollection workOrderServices = service.RetrieveMultiple(queryExpression);
                if (workOrderServices != null)
                {
                    Entity workOrderService = workOrderServices[0];

                    ServiceTask["msdyn_name"] = Name;
                    ServiceTask["msdyn_workorder"] = new EntityReference("msdyn_workorder", entity.Id);
                    ServiceTask["net_bylaw"] = byLaw;
                    ServiceTask["net_critical"] = Critical;
                    ServiceTask["net_element"] = new EntityReference("net_element", Element.Id);
                    ServiceTask["net_substandard"] = new EntityReference("net_substandard", SubStandard.Id);
                    //ServiceTask["net_testdate"] = DateTime.Now.AddDays(21);
                    ServiceTask["net_service"] = new EntityReference("msdyn_workorderservice", workOrderService.Id);
                    //ServiceTask["net_assignmentdate"] = DateTime.Now.AddDays(21);
                    ServiceTask["net_standard"] = new EntityReference("net_standard", Standard.Id);
                    ServiceTask["net_workordertype"] = new EntityReference("msdyn_workordertype", WorkOrderType.Id);

                    service.Create(ServiceTask);
                }
            }
        }

        private static EntityCollection GetRelatedStandards(IOrganizationService service, Entity educationalInstitute,
            EntityReference WorkOrderType, EntityReference VisitType)
        {
            //Get the school type of that school.
            EntityReference schoolType = (EntityReference)educationalInstitute["net_typeofinstitute"];

            string fetch = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='true'>
  <entity name='net_standard'>
    <attribute name='net_standardid' />
    <attribute name='net_name' />
    <attribute name='createdon' />
    <order attribute='net_name' descending='false' />
    <filter type='and'>
      <condition attribute='net_visittype' operator='eq' value='"+VisitType.Id+@"' />
      <condition attribute='net_institutetype' operator='eq' value='"+schoolType.Id+@"' />
    </filter>
    <link-entity name='net_standardandinspectiontype' from='net_standard' to='net_standardid' alias='ab'>
      <filter type='and'>
        <condition attribute='net_workordertype' operator='eq' value='"+WorkOrderType.Id+@"' />
      </filter>
    </link-entity>
  </entity>
</fetch>";

            EntityCollection standardList = service.RetrieveMultiple(new FetchExpression(fetch));
            return standardList;
        }

        private static EntityCollection GetRelatedSubStandards(IOrganizationService service, EntityCollection standardList, Entity entity)
        {
            EntityCollection subStandardList = new EntityCollection();

            foreach (Entity Standard in standardList.Entities)
            {
                #region Create Standard Instances
                //Create the Inspection Standard.
                Entity inspectionStandard = new Entity("net_inspectionstandard");

                string standardName = Standard.GetAttributeValue<string>("net_name");
                inspectionStandard["net_name"] = standardName;
                inspectionStandard["net_standard"] = new EntityReference("net_standard", Standard.Id);
                inspectionStandard["net_workorder"] = new EntityReference("msdyn_workorder", entity.Id);
                service.Create(inspectionStandard);
                #endregion

                #region Get All Domains
                //Query over all Domains that are related to that standard.
                ConditionExpression conditionExpression = new ConditionExpression();

                conditionExpression.AttributeName = "net_standard";
                conditionExpression.Operator = ConditionOperator.Equal;
                conditionExpression.Values.Add(Standard.Id);

                FilterExpression filterExpression = new FilterExpression();
                filterExpression.Conditions.Add(conditionExpression);

                QueryExpression queryExpression = new QueryExpression("net_substandard");
                queryExpression.ColumnSet = new ColumnSet(true);
                queryExpression.Criteria.AddFilter(filterExpression);

                //Retrieve a collection of sub standards related to that standard.
                EntityCollection partialSubStandardList = service.RetrieveMultiple(queryExpression);
                foreach (Entity subStandard in partialSubStandardList.Entities)
                {
                    subStandardList.Entities.Add(subStandard);
                }
                #endregion
            }
            return subStandardList;
        }

        private static EntityCollection GetRelatedElements(IOrganizationService service, EntityCollection subStandardList, Entity entity)
        {
            EntityCollection ElementList = new EntityCollection();
            foreach (Entity SubStandard in subStandardList.Entities)
            {
                //retrieve the parent standard
                EntityReference standard = SubStandard.GetAttributeValue<EntityReference>("net_standard");

                #region Get the only Standard Instance that is equivalent to the parent standard

                string fetch = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='net_inspectionstandard'>
    <attribute name='net_inspectionstandardid' />
    <attribute name='net_name' />
    <attribute name='createdon' />
    <order attribute='net_name' descending='false' />
    <filter type='and'>
      <condition attribute='net_standard' operator='eq' value='"+standard.Id+@"' />
      <condition attribute='net_workorder' operator='eq' value='"+entity.Id+@"' />
    </filter>
  </entity>
</fetch>";
                EntityCollection inspectionStandards = service.RetrieveMultiple(new FetchExpression(fetch));
                Entity inspectionStandard = new Entity("net_inspectionstandard");
                if(inspectionStandards != null)
                {
                    inspectionStandard = inspectionStandards[0];
                }
                #endregion

                #region Create the Domain Instances
                //Create the domain instances
                Entity WOService = new Entity("msdyn_workorderservice");

                string name = (string)SubStandard["net_name"];

                WOService["msdyn_name"] = name;
                WOService["net_substandard"] = new EntityReference("net_substandard", SubStandard.Id);
                WOService["msdyn_workorder"] = new EntityReference("msdyn_workorder", entity.Id);
                WOService["net_inspectionstandard"] = new EntityReference("net_inspectionstandard", inspectionStandard.Id);

                service.Create(WOService);
                #endregion

                #region Get All Elements related to that Domain
                //Query over all elements that are related to that Domain.
                ConditionExpression conditionExpression = new ConditionExpression();

                conditionExpression.AttributeName = "net_substandard";
                conditionExpression.Operator = ConditionOperator.Equal;
                conditionExpression.Values.Add(SubStandard.Id);

                FilterExpression filterExpression = new FilterExpression();
                filterExpression.Conditions.Add(conditionExpression);

                QueryExpression queryExpression = new QueryExpression("net_element");
                queryExpression.ColumnSet = new ColumnSet(true);
                queryExpression.Criteria.AddFilter(filterExpression);

                //Retrieve a collection of elements related to that sub standard.
                EntityCollection partialElementList = service.RetrieveMultiple(queryExpression);
                foreach (Entity Element in partialElementList.Entities)
                {
                    ElementList.Entities.Add(Element);
                }
                #endregion
            }
            return ElementList;
        }

        
    }
}
