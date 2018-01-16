function SubmitVisibility() {
    
    var FormType = Xrm.Page.ui.getFormType();
    // if form type not update don't show button
    if (FormType != 2) {
        return false;
    }
    //get the type of institute.
    var instituteType = Xrm.Page.getAttribute("net_institutetype").getValue()[0].name;
    if (instituteType == "Higher Education") {
        return false;
    }
    else {
        var Formstatus = Xrm.Page.getAttribute("statuscode").getValue();
        if (Formstatus == 1 || Formstatus == 279670003) {
            return true;
        }
        else {
            return false;
        }
    }
}

function SubmitAction() {
    debugger;
    //Xrm.Page.getAttribute("net_hastlsubmit").setValue(true);
    //Xrm.Page.data.entity.save();
    //Xrm.Page.getAttribute("net_hastlsubmit").fireOnChange();
    Xrm.Page.data.process.moveNext();
    Xrm.Page.ui.refreshRibbon();

    //Xrm.Page.data.refresh();


}

function Movenext() {
    debugger;
    Xrm.Page.getAttribute("net_hastlsubmit").fireOnChange();
    Xrm.Page.data.process.moveNext();
    Xrm.Page.ui.refreshRibbon();

}

function ReviewVisibility() {
  
    var FormType = Xrm.Page.ui.getFormType();
    // if form type not update don't show button
    if (FormType != 2) {
        return false;
    }
    //get the type of institute.
    var instituteType = Xrm.Page.getAttribute("net_institutetype").getValue()[0].name;
    if (instituteType == "Higher Education") {
        return false;
    }
    else {
        var Formstatus = Xrm.Page.getAttribute("statuscode").getValue();
        if (Formstatus == 279670000 || Formstatus == 279670001) {
            return true;
        }
        else {
            return false;
        }
    }
}

function ApproveVisibility() {
    var Formstatus = Xrm.Page.getAttribute("statuscode").getValue();
    if (Formstatus == 279670000 || Formstatus == 279670001) {
        return true;
    }
    else {
        return false;
    }
}

function ApproveAction() {
    Xrm.Page.data.process.moveNext();
    Xrm.Page.ui.refreshRibbon();
}

function RejectVisibility() {
    var Formstatus = Xrm.Page.getAttribute("statuscode").getValue();
    if (Formstatus == 279670000 || Formstatus == 279670001) {
        return true;
    }
    else {
        return false;
    }
}

function RejectAction() {
    debugger;
    var Formstatus = Xrm.Page.getAttribute("statuscode").getValue();
    if (Formstatus == 279670000) {
        Xrm.Page.data.process.movePrevious();
        Xrm.Page.ui.refreshRibbon();
    }

    else if (Formstatus == 279670001) {
        var GUID = "18519ba4-4ab4-4ebf-b736-7e5d9710b07d";
        Xrm.Page.data.process.setActiveStage(GUID, function (result) {
            if (result == "success") {
                return;
            } else {
                alert("An error occured. Please contact the administrator for support.");
            }
        });
    }
}

function SubmitHEVisibility() {
    var FormType = Xrm.Page.ui.getFormType();
    // if form type not update don't show button
    if (FormType != 2) {
        return false;
    }
    //get the type of institute.
    var instituteType = Xrm.Page.getAttribute("net_institutetype").getValue()[0].name;
    if (instituteType == "Higher Education") {
        var Formstatus = Xrm.Page.getAttribute("statuscode").getValue();
        if (Formstatus == 1 || Formstatus == 279670003 || Formstatus == 279670001) {
            return true;
        }
        else {
            return false;
        }
    }
    else {
        return false;
    }
}

function SubmitHEAction() {
    var Formstatus = Xrm.Page.getAttribute("statuscode").getValue();
    if (Formstatus == 1) {
        Xrm.Page.data.process.moveNext();
        Xrm.Page.ui.refreshRibbon();
    }
    else {
        var GUID = "f5f506db-1d35-4a3c-a6fd-e58135baec21";
        Xrm.Page.data.process.setActiveStage(GUID, function (result) {
            if (result == "success") {
                return;
            } else {
                alert("An error occured. Please contact the administrator for support.");
            }
        });
    }
}

function ReviewHEVisibility() {
    var FormType = Xrm.Page.ui.getFormType();
    // if form type not update don't show button
    if (FormType != 2) {
        return false;
    }
    //get the type of institute.
    var instituteType = Xrm.Page.getAttribute("net_institutetype").getValue()[0].name;
    if (instituteType == "Higher Education") {
        var Formstatus = Xrm.Page.getAttribute("statuscode").getValue();
        if (Formstatus == 279670000) {
            return true;
        }
        else {
            return false;
        }
    }
    else {
        return false;
    }
}

function SendToQA() {
    Xrm.Page.getAttribute("net_submittedto").setValue(2);
    Xrm.Page.data.entity.save();
    Xrm.Page.getAttribute("net_submittedto").fireOnChange();
    setTimeout(moveNext, 1000);
}

function SendToTranslator() {
    Xrm.Page.getAttribute("net_submittedto").setValue(1);
    Xrm.Page.data.entity.save();
    Xrm.Page.getAttribute("net_submittedto").fireOnChange();
    setTimeout(moveNext, 1000);
}

function Publish() {
    Xrm.Page.getAttribute("net_submittedto").setValue(3);
    Xrm.Page.data.entity.save();
    Xrm.Page.getAttribute("net_submittedto").fireOnChange();
    setTimeout(moveNext, 1000);
}

function ReturnHE() {
    Xrm.Page.data.process.movePrevious();
    Xrm.Page.ui.refreshRibbon();
}

function moveNext() {
    debugger;
    Xrm.Page.data.process.moveNext();
    Xrm.Page.ui.refreshRibbon();

}