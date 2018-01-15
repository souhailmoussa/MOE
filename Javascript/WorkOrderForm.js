function changeFormBasedonWorkOrderType() {
    debugger;
    var FormType = Xrm.Page.ui.getFormType();

    // if form type not update return
    if (FormType != 2) {
        return;
    }
    if (Xrm.Page.getAttribute("msdyn_workordertype") != null && Xrm.Page.getAttribute("msdyn_workordertype") != undefined) {
        var WorkOrderType = Xrm.Page.getAttribute("msdyn_workordertype").getValue()[0].name;
        if (WorkOrderType != null) {
            var currentForm = Xrm.Page.ui.formSelector.getCurrentItem();
            var inspectionVisitType = Xrm.Page.getAttribute("net_inspectionvisittype");
            if (inspectionVisitType != null && inspectionVisitType != undefined) {

                var VisitType = inspectionVisitType.getValue();
                if (VisitType != null) {
                    //if form is equal to work order type and visit is not a QA visit.
                    if (WorkOrderType == currentForm.getLabel() && Xrm.Page.getAttribute("net_inspectionvisittype").getValue()[0].name != "QA Report") {
                        return;
                    }

                    //check if it is a QA Report, navigate to the Inspection-QA form.
                    else if (Xrm.Page.getAttribute("net_inspectionvisittype").getValue()[0].name == "QA Report" && currentForm.getLabel() != "Inspection-QA") {
                        var AllForms = Xrm.Page.ui.formSelector.items.get();
                        for (var i = 0; i < AllForms.length; i++) {
                            var QAForm = "Inspection-QA";
                            if (QAForm == AllForms[i].getLabel()) {
                                AllForms[i].navigate();
                                return;
                            }
                        }
                    }
                    else if (Xrm.Page.getAttribute("net_inspectionvisittype").getValue()[0].name == "QA Report" && currentForm.getLabel() == "Inspection-QA") {
                        return;
                    }
                    else {
                        var AllForms = Xrm.Page.ui.formSelector.items.get();
                        for (var i = 0; i < AllForms.length; i++) {
                            if (WorkOrderType == AllForms[i].getLabel()) {
                                AllForms[i].navigate();
                                return;
                            }
                        }
                    }
                }

                else {
                    var AllForms = Xrm.Page.ui.formSelector.items.get();
                    for (var i = 0; i < AllForms.length; i++) {
                        if (WorkOrderType == AllForms[i].getLabel()) {
                            AllForms[i].navigate();
                            return;
                        }
                    }
                }
            }

            else if (WorkOrderType == currentForm.getLabel()) {
                return;
            }
            else {
                var AllForms = Xrm.Page.ui.formSelector.items.get();
                for (var i = 0; i < AllForms.length; i++) {
                    if (WorkOrderType == AllForms[i].getLabel()) {
                        AllForms[i].navigate();
                        return;
                    }
                }
            }
        }
    }
}