function LoadCommunities()
{
    $.ajax({
        type: "GET",
        url: '/AdminHelper/LoadCommunitiesList/',
        content: "application/json",
        dataType: "json",
        success: function (data) {
            var items = "<option value=''>All Communities</option>";
            $.each(data, function (i, community) {
                items += "<option value='" + community.Value + "'>" + community.Text + "</option>";
            });
            $('#CommunitiesFilterDDL').html(items);
        },
        complete: function () {
            GetFilterState();
        }
    });
};

function LoadGroupsList(communityID, groupID)
{
    if (communityID != "" && communityID != null) {
        $.ajax({
            type: "GET",
            url: '/AdminHelper/LoadGroupsList/',
            data: { "CommunityID": communityID },
            content: "application/json",
            dataType: "json",
            success: function (data) {
                var items = "<option value=''>All Groups</option>";
                $.each(data, function (i, group) {
                    items += "<option value='" + group.Value + "'>" + group.Text + "</option>";
                });
                   $('#GroupsFilterDDL').html(items);
            },
            complete: function () {
                $('#GroupsFilterDDL').val(groupID);
            }
        });
    }
    else {
        $('#GroupsFilterDDL').html("<option value=''>All Groups</option>");
    }
}

// Community DDL Change;
$(function () {
    $('#CommunitiesFilterDDL').change(function () {
        LoadGroupsList($("#CommunitiesFilterDDL :selected").val(), null);
    });
});

$(function () {
    $('#ApplyFilter').click(function () {
        SaveFilterState();
    });
});

// Save Filter State
function SaveFilterState()
{
    var communityID = $("#CommunitiesFilterDDL :selected").val();
    var groupID = $("#GroupsFilterDDL :selected").val();
    var includeAllSubGroups = $("#IncludeAllSubGroups").is(':checked');
    // Save Filter State
    $.ajax({
        type: "POST",
        url: '/AdminHelper/SaveFilterState/',
        data: { "communityID": communityID, "groupID" : groupID, "includeAllSubGroups" : includeAllSubGroups },
        content: "application/json",
        dataType: "json",
        success: function (result) {
            location.reload();
        }
    });
}

// Get Filter State
function GetFilterState() {
    $.ajax(
    {
        type: "GET",
        url: '/AdminHelper/GetFilterState/',
        content: "application/json",
        dataType: "json",
        success: function (result)
        {
            $('#CommunitiesFilterDDL').val(result.communityID);
            LoadGroupsList(result.communityID, result.groupID);
            $('#IncludeAllSubGroups').attr('checked', result.includeAllSubGroups);
        }
    });
}
