// notification.js — Phase 8: ASP.NET Core SignalR client
// Replaces jquery.signalR-2.x hub proxy ($.connection.notificationHub / $.connection.hub.start)
// with the @microsoft/signalr HubConnectionBuilder API.

var el = document.querySelector('.notification');

(function () {
    var connection = new signalR.HubConnectionBuilder()
        .withUrl("/notificationHub")
        .withAutomaticReconnect()
        .build();

    connection.on("addNotifcation", function (title, message, timestamp) {
        var count = Number(el.getAttribute('data-count')) || 0;
        el.setAttribute('data-count', count + 1);
        el.classList.remove('notify');
        el.offsetWidth = el.offsetWidth;
        el.classList.add('notify');
        if (count === 0) {
            el.classList.add('show-count');
        }

        $('#discussion').prepend(
            '<li class="card-body notify-body"><h5 class="card-title">' +
            '<strong>' + htmlEncode(title) + '</strong>' +
            '<i class="pull-right">' + htmlEncode(timestamp) + '</i>' +
            '</h4><p class="card-text">' + htmlEncode(message) + '</p></li>');

        updateNotifications(true);
        GetDeviceEventHistory();
    });

    connection.start().catch(function (err) {
        console.error('SignalR connection error:', err);
    });
})();

function updateNotifications(update) {
    var listItems = $("#discussion li").toArray();
    var showListItems = listItems.splice(0, 5);
    if (!update)
        $('#discussion li').remove();

    $(listItems).hide();
    $(showListItems).show();
}

function htmlEncode(value) {
    var encodedValue = $('<div />').text(value).html();
    return encodedValue;
}

$('#notify-dismiss-btn').click(function (e) {
    e.stopPropagation();
    el.setAttribute('data-count', 0);
    el.classList.remove('show-count');
    updateNotifications(false);
    document.getElementById('notifyIconbtn').classList.remove('open');
});
