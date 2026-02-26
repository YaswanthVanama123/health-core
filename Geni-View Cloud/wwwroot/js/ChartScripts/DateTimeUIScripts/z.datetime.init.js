// Set Localization...
$.datetimepicker.setLocale('en');

// Default 7 days range.
var beginDate = new Date();
beginDate.setDate(beginDate.getDate() - 7);

$('#BeginDate').datetimepicker({
    mask: '9999/19/39 29:59',
    dayOfWeekStart: 1,
    lang: 'en',
    value: beginDate,
    step: 5
});

$('#EndDate').datetimepicker({
    mask: '9999/19/39 29:59',
    dayOfWeekStart: 1,
    lang: 'en',
    value: new Date(),
    step: 5
});

//var Bday = dateFormat(new Date($('#fBeginDate').val()), 'yyyy/mm/dd HH:MM');

$('#fBeginDate').datetimepicker({
    mask: '9999/19/39 29:59',
    dayOfWeekStart: 1,
    lang: 'en',
    value: $('#fBeginDate').val(),
    step: 5
});

$('#fEndDate').datetimepicker({
    mask: '9999/19/39 29:59',
    dayOfWeekStart: 1,
    lang: 'en',
    value: $('#fEndDate').val(),
    step: 5
});