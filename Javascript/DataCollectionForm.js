function onLoad() {
    sectionSetDisabled("tab_2", "tab_2_section_1", true);
    sectionSetDisabled("tab_3", "tab_3_section_1", true);
    sectionSetDisabled("tab_5", "tab_5_section_1", true);
    sectionSetDisabled("tab_6", "tab_6_section_1", true);
}

function sectionSetDisabled(tabNumber, sectionNumber, disablestatus) {
    var section = Xrm.Page.ui.tabs.get(tabNumber).sections.get(sectionNumber);
    var controls = section.controls.get();
    var controlsLenght = controls.length;

    for (var i = 0; i < controlsLenght; i++) {
        controls[i].setDisabled(disablestatus)
    }
}