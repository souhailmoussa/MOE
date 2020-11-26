function onLoad() {
    debugger;
    var formType = Xrm.Page.ui.getFormType();
    //check if it is a bulk edit form.
    if (formType == "6") {

        //Get the Loged In User ID.
        var logedInUserId = Xrm.Page.context.getUserId();
        var ECETeamId = GetTeamId("ECE Team");
        var HETeamId = GetTeamId("Higher Education Team");

        var isHigherEducation = CheckUserInTeam(logedInUserId, HETeamId);
        var isEarlyChildHood = CheckUserInTeam(logedInUserId, ECETeamId);

        if (isHigherEducation || isEarlyChildHood) {
            Xrm.Page.ui.controls.get("net_term").setVisible(false);
        }
        else {
            Xrm.Page.ui.controls.get("net_term").setVisible(true);
        }

        //Show the inspection section
        Xrm.Page.ui.tabs.get("tab_3").sections.get("inspection").setVisible(true);

        //Hide all other tabs and section on the bulk edit form
        Xrm.Page.ui.tabs.get("tab_3").sections.get("tab_3_section_1").setVisible(false);

        Xrm.Page.ui.tabs.get("tab_3").sections.get("tab_4_section_1").setVisible(false);

        Xrm.Page.ui.tabs.get("tab_3").sections.get("ADDRESS").setVisible(false);

        Xrm.Page.ui.tabs.get("tab_3").sections.get("SUMMARY_TAB_section_5").setVisible(false);

        Xrm.Page.ui.tabs.get("tab_3").sections.get("SUMMARY_TAB_section_6").setVisible(false);

        //Xrm.Page.ui.tabs.get("tab_3").sections.get("SUMMARY_TAB_section_9").setVisible(false);

        Xrm.Page.ui.tabs.get("tab_3").sections.get("SOCIAL_PANE_TAB").setVisible(false);

        Xrm.Page.ui.tabs.get("tab_4").setVisible(false);

        //Set start date and end date to required.
        Xrm.Page.getAttribute("net_startdate").setRequiredLevel("required");

        Xrm.Page.getAttribute("net_enddate").setRequiredLevel("required");

        //set the default date to today.
        var today = new Date();
        Xrm.Page.getAttribute("net_startdate").setValue(today);
        Xrm.Page.getAttribute("net_enddate").setValue(today);
        
    }
    else {
        return;
    }
}

function onChangeStartSelfCompliance() {

    var formType = Xrm.Page.ui.getFormType();
    //check if it is a bulk edit form.
    if (formType == "6") {
        var startSelfCompliance = Xrm.Page.getAttribute("net_startselfcompliance").getValue();
        var startInspection = Xrm.Page.getAttribute("net_startinspection").getValue();

        if (startSelfCompliance == 0) {
            Xrm.Page.ui.controls.get("net_startinspection").setVisible(true);
            Xrm.Page.ui.controls.get("net_inspectionvisittype").setVisible(true);
            Xrm.Page.ui.controls.get("net_term").setVisible(true);
        }
        else {
            Xrm.Page.ui.controls.get("net_startinspection").setVisible(false);
            Xrm.Page.ui.controls.get("net_inspectionvisittype").setVisible(false);
            Xrm.Page.ui.controls.get("net_term").setVisible(false);
        }
    }
    else {
        return;
    }
}

function onChangeStartInspection() {

    var formType = Xrm.Page.ui.getFormType();
    //check if it is a bulk edit form.
    if (formType == "6") {
        var startInspection = Xrm.Page.getAttribute("net_startinspection").getValue();

        if (startInspection == 0) {
            Xrm.Page.ui.controls.get("net_startselfcompliance").setVisible(true);
        }
        else {
            Xrm.Page.ui.controls.get("net_startselfcompliance").setVisible(false);
            Xrm.Page.ui.controls.get("net_inspectionvisittype").setVisible(true);
            Xrm.Page.ui.controls.get("net_term").setVisible(true);
        }
    }
    else {
        return;
    }
}

function onChangeStartDate() {
    var today = new Date();
    today.setHours(0, 0, 0, 0);
    var startDate = Xrm.Page.getAttribute("net_startdate").getValue();
    var day = startDate.getDay();
    
    if (startDate != null) {
        startDate.setHours(0, 0, 0, 0);
        if (startDate < today) {
            alert("Start date can not be less than today. لا يمكن أن يكون تاريخ البدء قبل تاريخ اليوم");
            Xrm.Page.getAttribute("net_startdate").setValue(today);
            Xrm.Page.getAttribute("net_enddate").setValue(today);
        }
        else {
            Xrm.Page.getAttribute("net_enddate").setValue(startDate);
        }
    }
    if (day == "5" || day == "6") {
        alert("Inspection can not be done on friday and saturday, please specify another day. لا يمكن إجراء الزيارات الرقابية خلال يومي الجمعة والسبت، يرجى تحديد يوماً آخر");
        Xrm.Page.getAttribute("net_startdate").setValue(today);
        Xrm.Page.getAttribute("net_enddate").setValue(today);
    }

    var isHoliday = checkIfHoliday(startDate);
    if (isHoliday) {
        alert("You can not schedule an inspection during a holiday. لا يمكن إجراء الزيارات الرقابية خلال أيام العطل الرسمية");
        Xrm.Page.getAttribute("net_startdate").setValue(today);
        Xrm.Page.getAttribute("net_enddate").setValue(today);
    }
}

function onChangeEndDate() {
    var startDate = Xrm.Page.getAttribute("net_startdate").getValue();
    var endDate = Xrm.Page.getAttribute("net_enddate").getValue();

    if (endDate < startDate) {
        alert("End date must be greater than start date. يجب أن يكون تاريخ الانتهاء بعد تاريخ البدء");
        Xrm.Page.getAttribute("net_enddate").setValue(startDate);
    }
    else {
        return;
    }
}

function preventAutoSave(econtext) {
    var eventArgs = econtext.getEventArgs();
    if (eventArgs.getSaveMode() == 70 || eventArgs.getSaveMode() == 2) {
        eventArgs.preventDefault();
    }
}

function checkIfHoliday(startDate) {
    debugger;
    var isHoliday = false;
    startDate.setDate(startDate.getDate() + 1);
    var startTimeISO = startDate.toISOString();
    var startTimeFormat = startTimeISO.replace(startTimeISO.substring(19, 23), "");

    var addOneStartDate = Xrm.Page.getAttribute("net_startdate").getValue();
    var addOneStartTimeISO = addOneStartDate.toISOString();
    var addOneStartTimeFormat = addOneStartTimeISO.replace(startTimeISO.substring(19, 23), "");

    var req = new XMLHttpRequest();
    req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v8.2/net_holidaies?$filter=net_startdate le (" + addOneStartTimeFormat + ") and  net_enddate ge (" + addOneStartTimeFormat + ")&$count=true", false);
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
                    isHoliday = true;
                }
            } else {
                Xrm.Utility.alertDialog(this.statusText);
            }
        }
    };
    req.send();
    return isHoliday;
}

function startEvaluationVisibility() {
    return false;
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