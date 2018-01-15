function FormOnLoad() {
    collapseOpporBusinessProcess();
    ShowHideNotes();
}
function FormOnSave() {
    RefreshRibbonIfCreate();
}
function collapseOpporBusinessProcess() {
    var FormType = Xrm.Page.ui.getFormType();
    if (FormType == 1) {
        Xrm.Page.ui.process.setVisible(false);
    }
    else {
        var CaseType = Xrm.Page.getAttribute("casetypecode");
        if (CaseType.getValue() == 2) {
            Xrm.Page.ui.process.setVisible(true);


        }
    }
    setTimeout(collapseOpporBusinessProcessDelay, 300)
}

function collapseOpporBusinessProcessDelay() {
    Xrm.Page.ui.process != null && Xrm.Page.ui.process.setDisplayState("collapsed");
}
function RefreshRibbonIfCreate() {
    var FormType = Xrm.Page.ui.getFormType();
    if (FormType == 1) {
        var CaseType = Xrm.Page.getAttribute("casetypecode");
        if (CaseType.getValue() == 2) {
            Xrm.Page.ui.process.setVisible(true);

        }
    }
}

function OnchangeOwner() {
    var Owner = Xrm.Page.getAttribute("ownerid").getValue()[0];
    if (Owner.entityType == "team") {
        if (Owner.name == "Team Leads") {
            var CurrentStageId = Xrm.Page.data.process.getActiveStage().getId();
            if (CurrentStageId == "1d798e89-c774-4081-a64e-1a4eaf1643d4") {
                Xrm.Page.data.save().then(MoveNextCallBack);
            }
        }
    }
    else if (Owner.entityType == "systemuser") {
        var Ownerid = Owner.id;
        var TeamLeadId = GetTeamId("Team Leads");
        var IsUserInTeam = CheckUserInTeam(Ownerid, TeamLeadId);
        if (IsUserInTeam) {
            var CurrentStageId = Xrm.Page.data.process.getActiveStage().getId();
            if (CurrentStageId == "1d798e89-c774-4081-a64e-1a4eaf1643d4") {
                Xrm.Page.data.save().then(MoveNextCallBack);
            }
        }
    }
}

function MoveNextCallBack() {
    Xrm.Page.data.process.moveNext();
    Xrm.Page.ui.refreshRibbon();
}

function ShowHideNotes() {
    var Owner = Xrm.Page.getAttribute("ownerid").getValue()[0];
    if (Owner.entityType == "team") {
        if (Owner.name == "Section Head Team" || Owner.name == "Directors Team") {
            Xrm.Page.ui.tabs.get("general").getSections().get("note_section").setVisible(true);
        }
        else {
            Xrm.Page.ui.tabs.get("general").getSections().get("note_section").setVisible(false);
        }
    }
    else if (Owner.entityType == "systemuser") {
        var Ownerid = Owner.id;
        var SectionHeadId = GetTeamId("Section Head Team");
        var IsUserInTeam = CheckUserInTeam(Ownerid, SectionHeadId);
        if (IsUserInTeam) {
            Xrm.Page.ui.tabs.get("general").getSections().get("note_section").setVisible(true);
        }
        else {
            var DirectorId = GetTeamId("Directors Team");
            var IsUserInTeam = CheckUserInTeam(Ownerid, DirectorId);
            if (IsUserInTeam) {
                Xrm.Page.ui.tabs.get("general").getSections().get("note_section").setVisible(true);
            }
            else {
                Xrm.Page.ui.tabs.get("general").getSections().get("note_section").setVisible(false);

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