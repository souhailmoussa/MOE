function onLoad() {
    var formType = Xrm.Page.ui.getFormType();
    //check if it is a bulk edit form.
    if (formType == "6") {

        //Show the inspection section
        Xrm.Page.ui.tabs.get("tab_3").sections.get("inspection").setVisible(true);

        //Hide all other tabs and section on the bulk edit form
        Xrm.Page.ui.tabs.get("tab_3").sections.get("tab_3_section_1").setVisible(false);

        Xrm.Page.ui.tabs.get("tab_3").sections.get("tab_4_section_1").setVisible(false);

        Xrm.Page.ui.tabs.get("tab_3").sections.get("ADDRESS").setVisible(false);

        Xrm.Page.ui.tabs.get("tab_3").sections.get("SUMMARY_TAB_section_5").setVisible(false);

        Xrm.Page.ui.tabs.get("tab_3").sections.get("SUMMARY_TAB_section_6").setVisible(false);

        Xrm.Page.ui.tabs.get("tab_3").sections.get("SUMMARY_TAB_section_9").setVisible(false);

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
    var startDate = Xrm.Page.getAttribute("net_startdate").getValue();
    var today = new Date();
    today.setHours(0, 0, 0, 0);
    if (startDate != null) {
        startDate.setHours(0, 0, 0, 0);
        if (startDate < today) {
            alert("Start date can not be less than today");
            Xrm.Page.getAttribute("net_startdate").setValue(today);
            Xrm.Page.getAttribute("net_enddate").setValue(today);
        }
        else {
            Xrm.Page.getAttribute("net_enddate").setValue(startDate);
        }
    }
}

function onChangeEndDate() {
    var startDate = Xrm.Page.getAttribute("net_startdate").getValue();
    var endDate = Xrm.Page.getAttribute("net_enddate").getValue();

    if (endDate < startDate) {
        alert("End date must be greater than start date");
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