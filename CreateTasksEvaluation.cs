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
    public class CreateTasksEvaluation : CodeActivity
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

                // Get Related Education Institution
                Entity Institution = service.Retrieve("account", entity.GetAttributeValue<EntityReference>("msdyn_serviceaccount").Id, new ColumnSet("net_principal"));
               
                //Get all summary ERF tasks.
                ConditionExpression SummaryERFConditionExpression = new ConditionExpression();

                SummaryERFConditionExpression.AttributeName = "net_type";
                SummaryERFConditionExpression.Operator = ConditionOperator.Equal;
                SummaryERFConditionExpression.Values.Add(1);

                FilterExpression SummaryERFFilterExpression = new FilterExpression();
                SummaryERFFilterExpression.Conditions.Add(SummaryERFConditionExpression);

                QueryExpression SummaryERFQueryExpression = new QueryExpression("net_evaluationtask");
                SummaryERFQueryExpression.ColumnSet = new ColumnSet(true);
                SummaryERFQueryExpression.Criteria.AddFilter(SummaryERFFilterExpression);

                EntityCollection evaluationTasks = service.RetrieveMultiple(SummaryERFQueryExpression);
                foreach (Entity evaluationTask in evaluationTasks.Entities)
                {
                    //create the summary erf record
                    Entity SummaryERFTask = new Entity("net_summaryerf");

                    string name = (string)evaluationTask["net_name"];
                    int subject = ((OptionSetValue)evaluationTask["net_subject"]).Value;

                    SummaryERFTask["net_name"] = name;
                    SummaryERFTask["net_subject"] = new OptionSetValue(subject);
                    SummaryERFTask["net_workorder"] = new EntityReference("msdyn_workorder", entity.Id);

                    service.Create(SummaryERFTask);
                }

                //Get all overall ERF tasks.
                ConditionExpression OverallERFConditionExpression = new ConditionExpression();

                OverallERFConditionExpression.AttributeName = "net_type";
                OverallERFConditionExpression.Operator = ConditionOperator.Equal;
                OverallERFConditionExpression.Values.Add(2);

                FilterExpression OverallERFFilterExpression = new FilterExpression();
                OverallERFFilterExpression.Conditions.Add(OverallERFConditionExpression);

                QueryExpression OverallERFQueryExpression = new QueryExpression("net_evaluationtask");
                OverallERFQueryExpression.ColumnSet = new ColumnSet(true);
                OverallERFQueryExpression.Criteria.AddFilter(OverallERFFilterExpression);

                EntityCollection overallEvaluationTasks = service.RetrieveMultiple(OverallERFQueryExpression);
                foreach (Entity overallEvaluationTask in overallEvaluationTasks.Entities)
                {
                    //create the summary erf record
                    Entity OverallERFTask = new Entity("net_overallerf");

                    string name = (string)overallEvaluationTask["net_name"];
                    int performanceIndicator = ((OptionSetValue)overallEvaluationTask["net_performanceindicator"]).Value;

                    OverallERFTask["net_name"] = name;
                    OverallERFTask["net_performanceindicator"] = new OptionSetValue(performanceIndicator);
                    OverallERFTask["net_workorder"] = new EntityReference("msdyn_workorder", entity.Id);

                    service.Create(OverallERFTask);
                }

                //Get all school Finding tasks.
                ConditionExpression SchoolFindingConditionExpression = new ConditionExpression();

                SchoolFindingConditionExpression.AttributeName = "net_type";
                SchoolFindingConditionExpression.Operator = ConditionOperator.Equal;
                SchoolFindingConditionExpression.Values.Add(3);

                FilterExpression SchoolFindingFilterExpression = new FilterExpression();
                SchoolFindingFilterExpression.Conditions.Add(SchoolFindingConditionExpression);

                QueryExpression SchoolFindingQueryExpression = new QueryExpression("net_evaluationtask");
                SchoolFindingQueryExpression.ColumnSet = new ColumnSet(true);
                SchoolFindingQueryExpression.Criteria.AddFilter(SchoolFindingFilterExpression);

                EntityCollection SchoolFindingEvaluationTasks = service.RetrieveMultiple(SchoolFindingQueryExpression);
                foreach (Entity SchoolFindingEvaluationTask in SchoolFindingEvaluationTasks.Entities)
                {
                    //create the School Information record
                    Entity SchoolFindingFTask = new Entity("net_schoolfinding");

                    string name = (string)SchoolFindingEvaluationTask["net_name"];
                    //int performanceIndicator = ((OptionSetValue)overallEvaluationTask["net_performanceindicator"]).Value;

                    SchoolFindingFTask["net_name"] = name;
                    //OverallERFTask["net_performanceindicator"] = new OptionSetValue(performanceIndicator);
                    SchoolFindingFTask["net_workorder"] = new EntityReference("msdyn_workorder", entity.Id);

                    service.Create(SchoolFindingFTask);
                }

                //Get all school information tasks.
                ConditionExpression SchoolInfoConditionExpression = new ConditionExpression();

                SchoolInfoConditionExpression.AttributeName = "net_type";
                SchoolInfoConditionExpression.Operator = ConditionOperator.Equal;
                SchoolInfoConditionExpression.Values.Add(4);

                FilterExpression SchoolInfoFilterExpression = new FilterExpression();
                SchoolInfoFilterExpression.Conditions.Add(SchoolInfoConditionExpression);

                QueryExpression SchoolInfoQueryExpression = new QueryExpression("net_evaluationtask");
                SchoolInfoQueryExpression.ColumnSet = new ColumnSet(true);
                SchoolInfoQueryExpression.Criteria.AddFilter(SchoolInfoFilterExpression);

                EntityCollection SchoolInfoEvaluationTasks = service.RetrieveMultiple(SchoolInfoQueryExpression);
                foreach (Entity SchoolInfoEvaluationTask in SchoolInfoEvaluationTasks.Entities)
                {
                    //create the School Information record
                    Entity SchoolInfoTask = new Entity("net_schoolreport");

                    string name = (string)SchoolInfoEvaluationTask["net_name"];
                    //int performanceIndicator = ((OptionSetValue)overallEvaluationTask["net_performanceindicator"]).Value;

                    SchoolInfoTask["net_name"] = name;
                    //net_educationalinstitution
                    SchoolInfoTask["net_principalname"] = Institution.GetAttributeValue<EntityReference>("net_principal").Name;
                  //  SchoolInfoTask["net_principalid"] = Institution.GetAttributeValue<EntityReference>("net_principal").Name;

                    
                    //OverallERFTask["net_performanceindicator"] = new OptionSetValue(performanceIndicator);
                    SchoolInfoTask["net_workorder"] = new EntityReference("msdyn_workorder", entity.Id);

                    service.Create(SchoolInfoTask);
                }

                //Get Pre-Evaluation Briefing tasks.
                ConditionExpression PreEvaluationConditionExpression = new ConditionExpression();

                PreEvaluationConditionExpression.AttributeName = "net_type";
                PreEvaluationConditionExpression.Operator = ConditionOperator.Equal;
                PreEvaluationConditionExpression.Values.Add(5);

                FilterExpression PreEvaluationFilterExpression = new FilterExpression();
                PreEvaluationFilterExpression.Conditions.Add(PreEvaluationConditionExpression);

                QueryExpression PreEvaluationQueryExpression = new QueryExpression("net_evaluationtask");
                PreEvaluationQueryExpression.ColumnSet = new ColumnSet(true);
                PreEvaluationQueryExpression.Criteria.AddFilter(PreEvaluationFilterExpression);

                EntityCollection PreEvaluationEvaluationTasks = service.RetrieveMultiple(PreEvaluationQueryExpression);
                foreach (Entity PreEvaluationEvaluationTask in PreEvaluationEvaluationTasks.Entities)
                {
                    //create the School Information record
                    Entity PreEvaluationTask = new Entity("net_preevaluationbriefing");

                    string name = (string)PreEvaluationEvaluationTask["net_name"];
                    //int performanceIndicator = ((OptionSetValue)overallEvaluationTask["net_performanceindicator"]).Value;

                    PreEvaluationTask["net_name"] = name;
                    PreEvaluationTask["net_nameoftheprincipal"] = Institution.GetAttributeValue<EntityReference>("net_principal");
                    //OverallERFTask["net_performanceindicator"] = new OptionSetValue(performanceIndicator);
                    PreEvaluationTask["net_workorder"] = new EntityReference("msdyn_workorder", entity.Id);

                    service.Create(PreEvaluationTask);
                }
            }
        }
    }
}
