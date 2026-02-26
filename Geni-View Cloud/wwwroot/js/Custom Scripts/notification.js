// Notification
var el = document.querySelector('.notification');
$(function () {
    var chat = $.connection.notificationHub;
    chat.client.addNotifcation = function (title, message, timestamp) {
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
    };
    $.connection.hub.start().done(function () {
    });

});

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