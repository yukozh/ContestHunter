"use strict";
(function () {
    var INITIAL_MSG_COUNT = 20;
    var EACH_SCROLL_COUNT = 10;

    var earliest, latest;
    var noEarlier = false, gettingEarlier = false;

    function getCommon(before, top, callback) {
        $.getJSON('/api/CommonChat?before=' + encodeURIComponent(before.toISOString()) + '&top=' + top, callback);
    }

    function msg2Div(msg) {
        return $('<div class="msg"/>')
                    .text('By ' + msg.User + '(' + new Date(msg.Time).toLocaleString() + '): ' + msg.Content);
    }

    function getEarlier() {
        if (noEarlier || gettingEarlier) {
            return;
        }
        gettingEarlier = true;
        getCommon(earliest, EACH_SCROLL_COUNT, function (data) {
            if (EACH_SCROLL_COUNT != data.length)
                noEarlier = true;
            if (data.length) {
                earliest = new Date(data[data.length - 1].Time);
                data = data.map(msg2Div);

                data.forEach(function (line) {
                    $('#msgs').append(line);
                    line.hide().slideDown();
                });
            }
            gettingEarlier = false;
        });
    }

    function postCommon(content) {
        $.post('/api/CommonChat/', { Content: content }, function () {
            $('#txtCommon').val('');
            $('#txtCommon').removeAttr('disabled');
        },'json');
    }

    function chatInit() {
        $('#msgWrapper').scroll(function () {
            if ($('#msgs').height() - $('#msgWrapper').height() - $('#msgWrapper').scrollTop() <= 50) {
                getEarlier();
            }
        });

        $('#txtContent').keypress(function (e) {
            if(e.ctrlKey && (e.keyCode==10 || e.keCode==13)){
                postCommon($(this).val());
                $(this).attr('disabled',true);
                return false;
            }
        });

        getCommon(new Date, INITIAL_MSG_COUNT, function (data) {
            if (data.length) {
                latest = new Date(data[0].Time);
                earliest = new Date(data[data.length - 1].Time);

                data = data.map(msg2Div);

                data.forEach(function (line) {
                    $('#msgs').append(line);
                });
            }
        });
    }
    window.Chat = {
        init: chatInit
    };
})();