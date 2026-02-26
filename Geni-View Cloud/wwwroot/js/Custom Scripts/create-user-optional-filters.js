$(function LoadRoleList() {
    var selRole = $('#EditModeRole').text();
    $.ajax({
        type: "GET",
        url: '/AdminHelper/LoadRolesList/',
        content: "application/json",
        dataType: "json",
        success: function (data) {
            var items;
            $.each(data, function (i, role) {
                items += "<option value='" + role.Value + "'>" + role.Text + "</option>";
            });
            $('#RolesDDL').html(items);
            if (selRole != null && selRole != "") {
                $('#RolesDDL').val(selRole);
            }
            // Rise DDL change event
            $('#RolesDDL').change();
        },
        error: function (result) {
            alert('Service call failed: ' + result.status + ' Type :' + result.statusText);
        }
    });
});

$(function () {
    // At Start Hide both Community and Group DropDown list
    $('#CommunityHideDiv').hide();
    $('#GroupHideDiv').hide();

    $('#RolesDDL').change(function () {
        var id = $("#RolesDDL :selected").val();
        // Application Admin and User Can see all Communities.
        if (id == "Application Admin" || id == "ApplicationUser")
        {
            $('#CommunityHideDiv').hide();
            $('#GroupHideDiv').hide();
        } else if (id == "Community Admin")
        {
            $('#CommunityHideDiv').show();
            $('#GroupHideDiv').hide();
        } else if (id == "Community Group Admin" || id == "Community User")
        {
            $('#CommunityHideDiv').show();
            $('#GroupHideDiv').show();
        }
    });
});
