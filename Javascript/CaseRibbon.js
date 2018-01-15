function SubmitVisibility() {
    debugger;
    var Formstatus = Xrm.Page.ui.getFormType();
    if (Formstatus != 2) {
        return false;
    }
    else {
        //  debugger;
        var CaseType = Xrm.Page.getAttribute("casetypecode");
        if (CaseType.getValue() == 2) {
            var Owner = Xrm.Page.getAttribute("ownerid").getValue()[0];
            if (Owner.entityType == "team") {
                if (Owner.name == "Team Leads") {
                    return true;
                }
                else {
                    return false;
                }
            }
            else if (Owner.entityType == "systemuser") {
                var Ownerid = Owner.id;
                var TeamLeadId = GetTeamId("Team Leads");
                var IsUserInTeam = CheckUserInTeam(Ownerid, TeamLeadId);
                if (IsUserInTeam) {
                    return true;
                }
                else {
                    return false;
                }
            }
        }
        else {
            return false;
        }
    }
}

function SubmitAction() {

    var CaseType = Xrm.Page.getAttribute("casetypecode");
    if (CaseType.getValue() == 2) {
        var Owner = Xrm.Page.getAttribute("ownerid").getValue()[0];
        if (Owner.entityType == "team") {
            if (Owner.name == "Team Leads") {
                Xrm.Page.data.save().then(MoveNextCallBack);
            }
        }
        else if (Owner.entityType == "systemuser") {
            var Ownerid = Owner.id;
            var TeamLeadId = GetTeamId("Team Leads");
            var IsUserInTeam = CheckUserInTeam(Ownerid, TeamLeadId);
            if (IsUserInTeam) {
                var ownerName = Owner.name;
                var ownerType = Owner.entityType;
                alert(ownerName);
                var fieldName = "net_teamleader";

                //Set the team leader field.
                SetLookUp(fieldName, ownerType, Ownerid, ownerName);

                Xrm.Page.data.save().then(MoveNextCallBack);
            }
        }
    }
}

function SetLookUp(fieldName, fieldType, fieldId, value) {
    try {
        var object = new Array();
        object[0] = new Object();
        object[0].id = fieldId;
        object[0].name = value;
        object[0].entityType = fieldType;
        Xrm.Page.getAttribute(fieldName).setValue(object);
    }
    catch (e) {
        alert("Error in SetLookUp: fieldName = " + fieldName + " fieldType = " + fieldType + " fieldId = " + fieldId + " value = " + value + " error = " + e);
    }
}

function ReturnTeamLeader() {
    var CaseType = Xrm.Page.getAttribute("casetypecode");
    if (CaseType.getValue() == 2) {
        var Owner = Xrm.Page.getAttribute("ownerid").getValue()[0];
        if (Owner.entityType == "team") {
            if (Owner.name == "Team Leads") {
                Xrm.Page.data.save().then(MovePreviousCallBack);
            }
        }
        else if (Owner.entityType == "systemuser") {
            var Ownerid = Owner.id;
            var TeamLeadId = GetTeamId("Team Leads");
            var IsUserInTeam = CheckUserInTeam(Ownerid, TeamLeadId);
            if (IsUserInTeam) {
                Xrm.Page.data.save().then(MovePreviousCallBack);
            }
        }
    }
}


function Reviewvisibility() {
    debugger;
    // set the review to only show for section head team or director team 
    var FormType = Xrm.Page.ui.getFormType();
    if (FormType != 2) {
        return false;
    }
    else {
        var CaseType = Xrm.Page.getAttribute("casetypecode");
        if (CaseType.getValue() == 2) {
            var Owner = Xrm.Page.getAttribute("ownerid").getValue()[0];
            if (Owner.entityType == "team") {
                if (Owner.name == "Section Head Team" || Owner.name == "Directors Team") {
                    return true;
                }
                else {
                    return false;
                }
            }
            else if (Owner.entityType == "systemuser") {
                var Ownerid = Owner.id;
                var SectionHeadId = GetTeamId("Section Head Team");
                var IsUserInTeam = CheckUserInTeam(Ownerid, SectionHeadId);
                if (IsUserInTeam) {
                    return true;
                }
                else {
                    var DirectorId = GetTeamId("Directors Team");
                    var IsUserInTeam = CheckUserInTeam(Ownerid, DirectorId);
                    if (IsUserInTeam) {
                        return true;
                    }
                    return false;
                }
            }
        }
        else {
            return false;
        }
    }
}

function ReviewAction() {

}


function Approvevisibility() {
    return true;
}

function ApproveAction() {
    var CaseType = Xrm.Page.getAttribute("casetypecode");
    if (CaseType.getValue() == 2) {
        var Owner = Xrm.Page.getAttribute("ownerid").getValue()[0];
        if (Owner.entityType == "team") {
            if (Owner.name == "Section Head Team") {
                Xrm.Page.data.save().then(MoveNextCallBack);
            }
            else if (Owner.name == "Directors Team") {
                var CaseId = Xrm.Page.data.entity.getId();
                ResolveCaseByDirector(CaseId);
                Xrm.Page.data.refresh();

            }

        }
        else if (Owner.entityType == "systemuser") {
            var Ownerid = Owner.id;
            var SectionHeadId = GetTeamId("Section Head Team");
            var IsUserInTeam = CheckUserInTeam(Ownerid, SectionHeadId);
            if (IsUserInTeam) {
                Xrm.Page.data.save().then(MoveNextCallBack);
            }
            else {
                debugger;
                var DirectorId = GetTeamId("Directors Team");
                var IsUserInTeam = CheckUserInTeam(Ownerid, DirectorId);
                if (IsUserInTeam) {
                    var CaseId = Xrm.Page.data.entity.getId();
                    ResolveCaseByDirector(CaseId);
                    Xrm.Page.data.refresh();
                }
            }
        }
    }
}


function Rejectvisibility() {
    return true;
}

function RejectAction() {
    debugger;
    var Owner = Xrm.Page.getAttribute("ownerid").getValue()[0];
    if (Owner.entityType == "team") {
        if (Owner.name == "Section Head Team") {
            var CaseId = Xrm.Page.data.entity.getId();
            UpdateCaseOwner(CaseId, "Team Leads");
            Xrm.Page.data.save();
            Xrm.Page.data.save().then(MovePreviousCallBack);
        }
        else if (Owner.name == "Directors Team") {
            var CaseId = Xrm.Page.data.entity.getId();
            UpdateCaseOwner(CaseId, "Section Head Team");
            Xrm.Page.data.save().then(MovePreviousCallBack);
        }
    }
    else if (Owner.entityType == "systemuser") {
        var Ownerid = Owner.id;
        var SectionHeadId = GetTeamId("Section Head Team");
        var IsUserInTeam = CheckUserInTeam(Ownerid, SectionHeadId);
        if (IsUserInTeam) {
            var CaseId = Xrm.Page.data.entity.getId();
            UpdateCaseOwner(CaseId, "Team Leads");            
            Xrm.Page.data.save().then(MovePreviousCallBack);
            debugger;
        }
        else {
            var DirectorId = GetTeamId("Directors Team");
            var IsUserInTeam = CheckUserInTeam(Ownerid, DirectorId);
            if (IsUserInTeam) {
                var CaseId = Xrm.Page.data.entity.getId();
                UpdateCaseOwner(CaseId, "Section Head Team");
                Xrm.Page.data.save().then(MovePreviousCallBack);
            }
        }
    }
}


function CheckUserInTeam(UserId, TeamId) {

    UserId = UserId.replace(/[{}]/g, '');
    TeamId = TeamId.replace(/[{}]/g, '');

    var UserInTeam = false;

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
                    UserInTeam = true;
                }


            } else {
                Xrm.Utility.alertDialog(this.statusText);
            }
        }
    };
    req.send();

    return UserInTeam;
}

function GetTeamId(TeamName) {

    var TeamId;

    var req = new XMLHttpRequest();
    req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v8.2/teams?$filter=name eq '" + TeamName + "'&$count=true", false);
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
                    var teamid = results.value[0].teamid;
                    TeamId = teamid;
                }

            } else {
                Xrm.Utility.alertDialog(this.statusText);
            }
        }
    };
    req.send();

    return TeamId;
}

function GetCaseInvestigations(CaseId) {

    var CaseHas2Investigations = false;
    var isTeamLead = false;
    var isSchoolPrin = false;

    CaseId = CaseId.replace(/[{}]/g, '');

    var req = new XMLHttpRequest();
    req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v8.2/net_investigations?$select=net_reportertype&$filter=_net_case_value eq " + CaseId + "&$count=true", false);
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
                if (recordCount >= 2) {
                    for (var i = 0; i < results.value.length; i++) {
                        var net_reportertype = results.value[i].net_reportertype;
                        if (net_reportertype == 1) {
                            isTeamLead = true;
                        }
                        else if (net_reportertype == 2) {
                            isSchoolPrin = true;
                        }
                    }
                }
                //for (var i = 0; i < results.value.length; i++) {
                //    var net_reportertype = results.value[i]["net_reportertype"];
                //    var net_reportertype_formatted = results.value[i]["net_reportertype@OData.Community.Display.V1.FormattedValue"];
                //}
            } else {
                Xrm.Utility.alertDialog(this.statusText);
            }
        }
    };
    req.send();

    if (isTeamLead && isSchoolPrin) {
        CaseHas2Investigations = true;
    }

    return CaseHas2Investigations;
}

function MoveNextCallBack() {
    Xrm.Page.data.process.moveNext();
    Xrm.Page.ui.refreshRibbon();
}

function MovePreviousCallBack() {
    Xrm.Page.data.process.movePrevious();
    Xrm.Page.ui.refreshRibbon();
}

function UpdateCaseOwner(CaseId,TeamName) {

    var GetTeamLeadId = GetTeamId(TeamName);//"");
    GetTeamLeadId = GetTeamLeadId.replace(/[{}]/g, '');

    CaseId = CaseId.replace(/[{}]/g, '');

    var entity = {};
    entity["ownerid@odata.bind"] = "/teams(" + GetTeamLeadId + ")";

    var req = new XMLHttpRequest();
    req.open("PATCH", Xrm.Page.context.getClientUrl() + "/api/data/v8.2/incidents(" + CaseId+")", false);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.onreadystatechange = function () {
        if (this.readyState === 4) {
            req.onreadystatechange = null;
            if (this.status === 204) {
                //Success - No Return Data - Do Something
            } else {
                Xrm.Utility.alertDialog(this.statusText);
            }
        }
    };
    req.send(JSON.stringify(entity));
}

function ResolveCaseByDirector(CaseId) {

    CaseId = CaseId.replace(/[{}]/g, '');

    var parameters = {};

    var req = new XMLHttpRequest();
    req.open("POST", Xrm.Page.context.getClientUrl() + "/api/data/v8.2/incidents(" + CaseId + ")/Microsoft.Dynamics.CRM.net_ResolveCase", false);
    req.setRequestHeader("OData-MaxVersion", "4.0");
    req.setRequestHeader("OData-Version", "4.0");
    req.setRequestHeader("Accept", "application/json");
    req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
    req.onreadystatechange = function () {
        if (this.readyState === 4) {
            req.onreadystatechange = null;
            if (this.status === 204) {
                //Success - No Return Data - Do Something
            } else {
                Xrm.Utility.alertDialog(this.statusText);
            }
        }
    };
    req.send(JSON.stringify(parameters));
}