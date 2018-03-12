function onLoad() {
    collapseOpporBusinessProcess();
    var FormType = Xrm.Page.ui.getFormType();
    if (FormType == 2) {
        //ShowHideAddqaDays();
        EnableComment();
        //  alert("hello");
        //var activeStage = Xrm.Page.data.process.getActiveStage();
        //alert(activeStage);
        //var stageID = Xrm.Page.getAttribute("stageid").getValue();
        //alert(stageID);
        //var instituteType = Xrm.Page.getAttribute("net_institutetype").getValue()[0].name;
        //if (instituteType == "Higher Education") {
        //    var processId = "C691B451-5D94-4405-B270-BFD01A831FB4"
        //    Xrm.Page.data.process.setActiveProcess(processId, function (result) {
        //        if (result == "success") {
        //            Xrm.Page.data.refresh();
        //        } else {
        //            alert("An error occured. Please contact the administrator for support.");
        //        }
        //    });
        //}
        
    }
}

function collapseOpporBusinessProcess() {
    setTimeout(collapseOpporBusinessProcessDelay, 300)
}

function collapseOpporBusinessProcessDelay() {
    Xrm.Page.ui.process != null && Xrm.Page.ui.process.setDisplayState("collapsed");
}

function EnableComment() {
    var UserId = Xrm.Page.context.getUserId()
    var ShteamId = getSHTeamId();

    UserId = UserId.replace(/[{}]/g, '');
    ShteamId = ShteamId.replace(/[{}]/g, '');

    var IsuserinSH = checkUserInTeam(UserId, ShteamId);
    if (IsuserinSH) {
        Xrm.Page.getControl("net_sectionheadcomment").setDisabled(false);
    }
    else {
        var QateamId = getQateamId();
        QateamId = QateamId.replace(/[{}]/g, '');

        var IsuserinQA = checkUserInTeam(UserId, QateamId);
        if (IsuserinQA) {
            Xrm.Page.getControl("net_qacomment").setDisabled(false);

        }
    }
}

function getSHTeamId() {



    var SHteamId;
    // get section head team id 
    var req = new XMLHttpRequest();
    req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v8.2/teams?$filter=name eq 'Section%20Head%20Team'&$count=true", false);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
    req.onreadystatechange = function () {
        if (this.readyState === 4) {
            req.onreadystatechange = null;
            if (this.status === 200) {
                var results = JSON.parse(this.response);
                debugger;
                var recordCount = results["@odata.count"];
                if (recordCount > 0) {
                    SHteamId = results.value[0].teamid;
                }

            } else {
                Xrm.Utility.alertDialog(this.statusText);
            }
        }
    };
    req.send();

    return SHteamId;
}

function getQateamId() {
    //get qa team id 

    var QateamId;

    var req = new XMLHttpRequest();
    req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v8.2/teams?$filter=name eq 'QA%20team'&$count=true", false);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
    req.onreadystatechange = function () {
        if (this.readyState === 4) {
            req.onreadystatechange = null;
            if (this.status === 200) {
                var results = JSON.parse(this.response);
                var recordCount = results["@odata.count"];
                if (recordCount > 0) {
                    QateamId = results.value[0].teamid;
                }

            } else {
                Xrm.Utility.alertDialog(this.statusText);
            }
        }
    };
    req.send();
    return QateamId;
}


function checkUserInTeam(UserId, TeamId) {

    var UserInteam = false;
    var req = new XMLHttpRequest();
    req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v8.2/teammemberships?$filter=systemuserid eq " + UserId + " and  teamid eq " + TeamId + "&$count=true", false);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.setRequestHeader("Prefer", "odata.include-annotations=\"*\"");
    req.onreadystatechange = function () {
        if (this.readyState === 4) {
            req.onreadystatechange = null;
            if (this.status === 200) {
                var results = JSON.parse(this.response);
                var recordCount = results["@odata.count"];
                if (recordCount > 0) {
                    UserInteam = true;
                }

            } else {
                Xrm.Utility.alertDialog(this.statusText);
            }
        }
    };
    req.send();

    return UserInteam;
}

function ShowHideAddqaDays() {
    debugger;
    var WorkOrderId = Xrm.Page.getAttribute("net_workorder").getValue()[0].id;
    var InstituteId = getEductionalInstitute(WorkOrderId);
    var TypeName = getInstituteType(InstituteId);
    if (TypeName != "Higher Education") {
        Xrm.Page.getControl("net_qaresponsedaysnumber").setVisible(true);
    }
    else {
        Xrm.Page.getControl("net_qaresponsedaysnumber").setVisible(false);

    }
}

function getEductionalInstitute(WorkOrderId) {
    debugger;

    WorkOrderId = WorkOrderId.replace(/[{}]/g, '');
    var InstituteId;

    var req = new XMLHttpRequest();
    req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v8.2/msdyn_workorders(" + WorkOrderId + ")?$select=_msdyn_serviceaccount_value", false);
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
                var _net_educationalinstitutetype_value = result._msdyn_serviceaccount_value;// result._net_educationalinstitutetype_value;
                //var _net_educationalinstitutetype_value_formatted = result["_net_educationalinstitutetype_value@OData.Community.Display.V1.FormattedValue"];
                //var _net_educationalinstitutetype_value_lookuplogicalname = result["_net_educationalinstitutetype_value@Microsoft.Dynamics.CRM.lookuplogicalname"];
                InstituteId = _net_educationalinstitutetype_value;
            } else {
                Xrm.Utility.alertDialog(this.statusText);
            }
        }
    };
    req.send();

    return InstituteId;
}

function getInstituteType(InstituteId) {

    InstituteId = InstituteId.replace(/[{}]/g, '');
    var TypeName;

    var req = new XMLHttpRequest();
    req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v8.2/accounts(" + InstituteId + ")?$select=_net_typeofinstitute_value", false);
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
                var _net_typeofinstitute_value = result["_net_typeofinstitute_value"];
                var _net_typeofinstitute_value_formatted = result["_net_typeofinstitute_value@OData.Community.Display.V1.FormattedValue"];
                var _net_typeofinstitute_value_lookuplogicalname = result["_net_typeofinstitute_value@Microsoft.Dynamics.CRM.lookuplogicalname"];
                TypeName = _net_typeofinstitute_value_formatted;
            } else {
                Xrm.Utility.alertDialog(this.statusText);
            }
        }
    };
    req.send();

    return TypeName;
}

function SetQaEndDate() {
    if (Xrm.Page.getAttribute("net_qaresponsedaysnumber") != null && Xrm.Page.getAttribute("net_qaresponsedaysnumber") != undefined) {

        if (Xrm.Page.getAttribute("net_qaresponsedaysnumber").getValue() != null && Xrm.Page.getAttribute("net_qaresponsedaysnumber").getValue() != undefined) {
            var qastartdate = Xrm.Page.getAttribute("net_qastartdate").getValue();
            //var qaDaysNumber = Xrm.Page.getAttribute("net_qaresponsedaysnumber").getValue();
            qastartdate.setDate(qastartdate.getDate() + Xrm.Page.getAttribute("net_qaresponsedaysnumber").getValue());
            Xrm.Page.getAttribute("net_qaenddate").setValue(qastartdate);
            Xrm.Page.data.entity.save();
        }
    }
}