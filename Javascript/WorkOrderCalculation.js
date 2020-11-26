function runInspectionCalculation() {
    Xrm.Page.getAttribute("net_runinspectioncalculation").setValue(true);
    Xrm.Page.data.save();
}

function runHealthSafetyCalculation() {
    Xrm.Page.getAttribute("net_runhealthandsafetycalculation").setValue(true);
    Xrm.Page.data.save();
}