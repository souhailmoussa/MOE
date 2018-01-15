function onLoad() {
    var lookupObject = Xrm.Page.getAttribute("net_workorder");

    if (lookupObject != null) {
        var lookUpObjectValue = lookupObject.getValue();
        if ((lookUpObjectValue != null)) {
            var lookupid = lookUpObjectValue[0].id;
        } // End if
    } // End if

    var req = new XMLHttpRequest();
    req.open("GET", Xrm.Page.context.getClientUrl() + "/api/data/v8.2/msdyn_workorders(" + lookupid.slice(1, -1) + ")?$select=_msdyn_serviceaccount_value", true);
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
                var _msdyn_serviceaccount_value = result["_msdyn_serviceaccount_value"];
                var _msdyn_serviceaccount_value_formatted = result["_msdyn_serviceaccount_value@OData.Community.Display.V1.FormattedValue"];
                var _msdyn_serviceaccount_value_lookuplogicalname = result["_msdyn_serviceaccount_value@Microsoft.Dynamics.CRM.lookuplogicalname"];
                alert(_msdyn_serviceaccount_value); //Return the ID of the institute
                alert(_msdyn_serviceaccount_value_formatted); //Return the name of the institute.
                alert(_msdyn_serviceaccount_value_lookuplogicalname); //Return the name of the entity (in this case, it is account).
            } else {
                Xrm.Utility.alertDialog(this.statusText);
            }
        }
    };
    req.send();
} //End function