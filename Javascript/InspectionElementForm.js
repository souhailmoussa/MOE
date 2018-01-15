function loadCorrectForm() {
    debugger;
    var FormType = Xrm.Page.ui.getFormType();
    var inspectionElementId = Xrm.Page.data.entity.getId().replace(/[{}]/g, '');
    var currentForm = Xrm.Page.ui.formSelector.getCurrentItem();

    // if form type not update return
    if (FormType != 2) {
        return;
    }
    var WorkOrder = Xrm.Page.getAttribute("msdyn_workorder");
    if (WorkOrder != null && WorkOrder != undefined) {
        var WorkOrderId = WorkOrder.getValue()[0].id;
        WorkOrderId = WorkOrderId.replace(/[{}]/g, '');
        //alert(WorkOrderId);

        var req = new XMLHttpRequest();
        req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v8.2/msdyn_workorders("+WorkOrderId+")?$select=_msdyn_workordertype_value&$expand=msdyn_workordertype($select=net_code)", true);
        req.setRequestHeader("OData-MaxVersion", "4.0");
        req.setRequestHeader("OData-Version", "4.0");
        req.setRequestHeader("Accept", "application/json");
        req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
        req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
        req.onreadystatechange = function () {
            if (this.readyState === 4) {
                req.onreadystatechange = null;
                if (this.status === 200) {
                    var result = JSON.parse(this.response);
                    var _msdyn_workordertype_value = result["_msdyn_workordertype_value"];
                    var _msdyn_workordertype_value_formatted = result["_msdyn_workordertype_value@OData.Community.Display.V1.FormattedValue"];
                    var _msdyn_workordertype_value_lookuplogicalname = result["_msdyn_workordertype_value@Microsoft.Dynamics.CRM.lookuplogicalname"];
                    if (result.hasOwnProperty("msdyn_workordertype")) {
                        var msdyn_workordertype_net_code = result["msdyn_workordertype"]["net_code"];

                        //if work order is health and safety.
                        if (msdyn_workordertype_net_code == "003") {

                            //if health and safety form is opened, then return.
                            if (currentForm.getLabel() == "Health & Safety") {
                                return;
                            }
                            
                            else {
                                //Open the Health & Safety form.
                                var parameters = {};
                                parameters["formid"] = "74B4DE62-EDAD-440E-96E2-378FD47DCA41";
                                var windowOptions = {
                                    openInNewWindow: false
                                };
                                Xrm.Utility.openEntityForm("msdyn_workorderservicetask", inspectionElementId, parameters, windowOptions);
                            }
                        }

                        //if work order is performance compliance.
                        else if (msdyn_workordertype_net_code == "010") {
                            //if performance compliance form is opened, then return.
                            if (currentForm.getLabel() == "Performance Compliance") {
                                return;
                            }

                            else {
                                //Open the Performance Compliance form.
                                var parameters = {};
                                parameters["formid"] = "EC49296B-B929-4480-8258-8DA839F07422";
                                var windowOptions = {
                                    openInNewWindow: false
                                };
                                Xrm.Utility.openEntityForm("msdyn_workorderservicetask", inspectionElementId, parameters, windowOptions);
                            }
                        }


                        //if work order is not health and safety
                        else {
                            //if Information form is opened, then return
                            if (currentForm.getLabel() == "Information") {
                                return;
                            }
                            else {
                                //open the Information form
                                var parameters = {};
                                parameters["formid"] = "12A5BE6B-B565-4D80-A2BC-3D4F652A3D98";
                                var windowOptions = {
                                    openInNewWindow: false
                                };
                                Xrm.Utility.openEntityForm("msdyn_workorderservicetask", inspectionElementId, parameters, windowOptions);
                            }
                        }
                    }
                } else {
                    Xrm.Utility.alertDialog(this.statusText);
                }
            }
        };
        req.send();
    }
}