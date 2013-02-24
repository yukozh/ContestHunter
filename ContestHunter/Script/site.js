
function RefreshValidationMessage() {
    $('span.field-validation-valid').each(function () {
        var cgroup = $(this);
        while (cgroup.length > 0 && !cgroup.hasClass('control-group'))
            cgroup = cgroup.parent();
        if (cgroup.hasClass('control-group')) {
            cgroup.removeClass('error');
        }
    })
    $('span.field-validation-error').each(function () {
        var cgroup = $(this);
        while (cgroup.length > 0 && !cgroup.hasClass('control-group'))
            cgroup = cgroup.parent();
        if (cgroup.hasClass('control-group')) {
            cgroup.addClass('error');
        }
    })
}

$(function () {
    $('input,textarea,select').filter('[data-val-required]').attr('required', 'required');

    $('span.field-validation-valid, span.field-validation-error').each(function () {
        $(this).wrap('<div style="margin: 5px; text-align: right;"/>');
        $(this)
            .addClass('label')
            .addClass('label-important')
            .css({
                'font-size': 'medium',
            });
        RefreshValidationMessage();
    })
});