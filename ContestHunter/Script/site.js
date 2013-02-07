function ShowValidationMessage(input,msg) {
    var controlGroup = input.parent().parent();
    controlGroup.addClass('error');
    var span = $('span.field-validation-valid, span.field-validation-error', controlGroup);
    span
        .show()
        .addClass('label')
        .addClass('label-important')
        .css({
            'font-size': 'medium',
        });
    if (msg !== undefined) {
        span.text(msg);
    }
}

function HideValidationMessage(input) {
    var controlGroup = input.parent().parent();
    controlGroup.removeClass('error');
    var span = $('span.field-validation-valid, span.field-validation-error', controlGroup);
    span.hide();
}

$(function () {
    /*
    $('input.input-validation-error').each(function () {
        var parent = $(this).parent().parent();
        parent.addClass('error');
    });
    */
    $('span.field-validation-valid, span.field-validation-error').each(function () {
        $(this).wrap('<div style="margin: 5px; text-align: right;"/>');
        var controlGroup = $(this).parent().parent().parent();
        var input = $('input',controlGroup);
        if ($(this).hasClass('field-validation-error')) {
            ShowValidationMessage(input);
        } else {
            HideValidationMessage(input);
        }
    })
});