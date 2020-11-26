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

/// <summary>
/// this class is done to share with the section head the data collection related to that specific inspection.
/// </summary>


namespace MOE.CustomActivity
{
    public class ShareDataCollectionWithSectionHead : CodeActivity
    {

        #region "Parameter Definition"

        /// <summary>
        /// Workflow input
        /// </summary>
        [RequiredArgument]
        [Input("Inspection Data Collection")]
        [ReferenceTarget("net_inspectiondatacollection")]
        public InArgument<EntityReference> inspectionDataCollection
        {
            get;
            set;
        }

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

        /// <summary>
        /// Share Read privilege.
        /// </summary>
        [Input("Read Permission")]
        [Default("True")]
        public InArgument<bool> ShareRead { get; set; }

        /// <summary>
        /// Share Write privilege.
        /// </summary>
        [Input("Write Permission")]
        [Default("False")]
        public InArgument<bool> ShareWrite { get; set; }

        /// <summary>
        /// Share Delete privilege.
        /// </summary>
        [Input("Delete Permission")]
        [Default("False")]
        public InArgument<bool> ShareDelete { get; set; }

        /// <summary>
        /// Share Append privilege.
        /// </summary>
        [Input("Append Permission")]
        [Default("False")]
        public InArgument<bool> ShareAppend { get; set; }

        /// <summary>
        /// Share AppendTo privilege.
        /// </summary>
        [Input("Append To Permission")]
        [Default("False")]
        public InArgument<bool> ShareAppendTo { get; set; }

        /// <summary>
        /// Share Assign privilege.
        /// </summary>
        [Input("Assign Permission")]
        [Default("False")]
        public InArgument<bool> ShareAssign { get; set; }

        /// <summary>
        /// Share Share privilege.
        /// </summary>
        [Input("Share Permission")]
        [Default("False")]
        public InArgument<bool> ShareShare { get; set; }
        #endregion

        protected override void Execute(CodeActivityContext activityContext)
        {
            //Create the tracing service.
            ITracingService tracingService = activityContext.GetExtension<ITracingService>();

            //Create the context.
            IWorkflowContext workflowContext = activityContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = activityContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(workflowContext.UserId);

            //Check the InArgument value
            if (this.inspectionDataCollection != null && this.Arg_WO != null)
            {
                //Get the Data Collection Record.
                Entity inspectionDataCollection = (Entity)service.Retrieve("net_inspectiondatacollection", this.inspectionDataCollection.Get<EntityReference>(activityContext).Id, new ColumnSet(true));
                
                //Get the Work Order Record.
                Entity workOrder = (Entity)service.Retrieve("msdyn_workorder", this.Arg_WO.Get<EntityReference>(activityContext).Id, new ColumnSet(true));

                EntityReference sectionHead = workOrder.GetAttributeValue<EntityReference>("net_sectionhead");

                ShareRecord(sectionHead.Id, inspectionDataCollection, service, activityContext);
            }
        }

        //Share function
        private void ShareRecord(Guid userId, Entity task, IOrganizationService service, CodeActivityContext activityContext)
        {
            GrantAccessRequest grantRequest = new GrantAccessRequest()
            {

                Target = new EntityReference(task.LogicalName, task.Id),
                PrincipalAccess = new PrincipalAccess()
                {
                    Principal = new EntityReference("systemuser", userId),
                    AccessMask = (AccessRights)getMask(activityContext)
                }
            };

            // Execute the request.
            GrantAccessResponse grantResponse =
                (GrantAccessResponse)service.Execute(grantRequest);
        }

        UInt32 getMask(CodeActivityContext executionContext)
        {
            bool ShareAppend = this.ShareAppend.Get(executionContext);
            bool ShareAppendTo = this.ShareAppendTo.Get(executionContext);
            bool ShareAssign = this.ShareAssign.Get(executionContext);
            bool ShareDelete = this.ShareDelete.Get(executionContext);
            bool ShareRead = this.ShareRead.Get(executionContext);
            bool ShareShare = this.ShareShare.Get(executionContext);
            bool ShareWrite = this.ShareWrite.Get(executionContext);

            UInt32 mask = 0;
            if (ShareAppend)
            {
                mask |= (UInt32)AccessRights.AppendAccess;
            }
            if (ShareAppendTo)
            {
                mask |= (UInt32)AccessRights.AppendToAccess;
            }
            if (ShareAssign)
            {
                mask |= (UInt32)AccessRights.AssignAccess;
            }

            if (ShareDelete)
            {
                mask |= (UInt32)AccessRights.DeleteAccess;
            }
            if (ShareRead)
            {
                mask |= (UInt32)AccessRights.ReadAccess;
            }
            if (ShareShare)
            {
                mask |= (UInt32)AccessRights.ShareAccess;
            }
            if (ShareWrite)
            {
                mask |= (UInt32)AccessRights.WriteAccess;
            }



            return mask;

        }
    }
}
