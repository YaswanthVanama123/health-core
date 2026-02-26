// Load Communities for RemoveBatteries
$(function () {
    var selCommunity = parseInt($('#EditModeCommunityID').text());
    $.ajax({
        type: "GET",
        url: '/AdminHelper/LoadCommunitiesList/',
        content: "application/json",
        dataType: "json",
        success: function (data) {
            var items= '<option value="0">Select Community</option>';
            $.each(data, function (i, community) {
                items += "<option value='" + community.Value + "'>" + community.Text + "</option>";
            });
            $('#communitydropdown').html(items);
            if (!isNaN(selCommunity)) {
                $('#communitydropdown').val(selCommunity);
            }
        },
        error: function (result) {
            alert('Service call failed: ' + result.status + ' Type :' + result.statusText);
        }
    });
});

// Load Communities for Users, Groups, and Dashboard
$(function () {
    var selCommunity = parseInt($('#EditModeCommunityID').text());
    $.ajax({
        type: "GET",
        url: '/AdminHelper/LoadCommunitiesList/',
        content: "application/json",
        dataType: "json",
        success: function (data) {
            var items;
            $.each(data, function (i, community) {
                items += "<option value='" + community.Value + "'>" + community.Text + "</option>";
            });
            $('#CommunitiesDDL').html(items);
            if (!isNaN(selCommunity)) {
                $('#CommunitiesDDL').val(selCommunity);
            }
            $('#CommunitiesDDL').change();
        },
        error: function (result) {
            alert('Service call failed: ' + result.status + ' Type :' + result.statusText);
        }
    });
});

$(function () {
    $('#CommunitiesDDL').change(function () {
        var id = $("#CommunitiesDDL :selected").val();
        var selGroup = parseInt($('#EditModeParentGroupID').text());
        if (id != "" && id != null) {
            $.ajax({
                type: "GET",
                url: '/AdminHelper/LoadGroupsList/',
                data: { "CommunityID": id },
                content: "application/json",
                dataType: "json",
                success: function (data) {
                    var items = '<option value="">None</option>';
                    $.each(data, function (i, group) {
                        items += "<option value='" + group.Value + "'>" + group.Text + "</option>";
                    });
                    $('#GroupsDDL').html(items);
                    if (!isNaN(selGroup)) {
                        $('#GroupsDDL').val(selGroup);
                    }
                },
                error: function (result) {
                    alert('Service call failed: ' + result.status + ' Type :' + result.statusText);
                }
            });
        }
        else {
            $('#GroupsDDL').html('<option value="">Please, Select Community</option>');
        }
    });
});
