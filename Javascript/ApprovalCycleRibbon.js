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
        //If status == initializing or status = pending with translator.
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
        // if status = pending with section or status == pending with qa.
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
    // if status = pending with section or status == pending with qa.
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
    // if status = pending with section or status == pending with qa.
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
    //if status == Pending with Section Head
    if (Formstatus == 279670000) {
        //Return to the team leader stage.
        var GUID = "b9290312-0294-4121-95f7-1fbc66a786bd";
        Xrm.Page.data.process.setActiveStage(GUID, function (result) {
            if (result == "success") {
                return;
            } else {
                alert("An error occured. Please contact the administrator for support.");
            }
        });
    }
    //if status == Pending with QA.
    else if (Formstatus == 279670001) {
        //return to the section head stage.
        Xrm.Page.data.process.movePrevious();
        Xrm.Page.ui.refreshRibbon();
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
        //Initializing: 1
        //Pending With Translator: 279,670,003
        //Pending with QA: 279,670,001
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

//Need to change the stage Guid for authorized user stage.
function SubmitHEAction() {
    var Formstatus = Xrm.Page.getAttribute("statuscode").getValue();
    //Initializing: 1
    if (Formstatus == 1) {
        Xrm.Page.data.process.moveNext();
        Xrm.Page.ui.refreshRibbon();
    }
    else {
        //move to the authorized user stage.
        Xrm.Page.data.process.movePrevious();
        Xrm.Page.ui.refreshRibbon();
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
        //Pending with Section Head: 279,670,000
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