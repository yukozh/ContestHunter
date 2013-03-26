"use strict";
(function () {
    var INITIAL_MSG_COUNT = 20;
    var EACH_SCROLL_COUNT = 10;

    var currentUser;
    var earliest, latest;
    var noEarlier = false, gettingEarlier = false;
    var socket;

    function getCommon(before, top, callback) {
        $.getJSON('/api/CommonChat?before=' + encodeURIComponent(before.toISOString()) + '&top=' + top, callback);
    }

    function msg2Div(msg) {
        if (currentUser != msg.User) {
            return $('<div class="inner"/>')
                .append($('<div class="image-inner left"/>')
                    .append($('<img alt="" style="width:50px; height:50px;"/>')
                        .attr('src', msg.UserImg)))
                .append($('<fieldset class="fieldset-inner right"/>')
                    .append($('<legend class="legend-inner"/>')
                        .append($('<a/>')
                            .attr('href', '/User/Show/' + msg.User)
                            .text(msg.User))
                        .append(' ')
                        .append(new Date(msg.Time).toLocaleString()))
                    .append($('<div/>').text(msg.Content)))
                .append('<div style="clear: both;"/>');
        } else {
            return $('<div class="inner"/>')
                .append($('<div class="image-inner right"/>')
                    .append($('<img alt="" style="width:50px; height:50px;"/>')
                        .attr('src', msg.UserImg)))
                .append($('<fieldset class="fieldset-inner right-text"/>')
                    .append($('<legend class="legend-inner right-text"/>')
                        .append($('<a/>')
                            .attr('href', '/User/Show/' + msg.User)
                            .text(msg.User))
                        .append(' ')
                        .append(new Date(msg.Time).toLocaleString()))
                    .append($('<div/>').text(msg.Content)))
                .append('<div style="clear: both;"/>');
        }
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
        $.ajax({
            url: '/api/CommonChat',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ Content: content }),
            success: function () {
                $('#txtContent').val('');
                $('#txtContent').removeAttr('disabled');
            },
            error: function () {
                alert('发送消息时出错');
            }
        });
    }

    function chatPost() {
        postCommon($('#txtContent').val());
        $('#txtContent').attr('disabled', true);
    }

    function socketOnMessage(e) {
        var msg = JSON.parse(e.data);
        console.log(msg);
        switch (msg.Type) {
            case 'CommonMessage':
                var div = msg2Div(msg.Message);
                $('#msgs').prepend(div);
                div.hide().fadeIn();
                break;
        }
    }

    function socketOnError(e) {
    }

    function socketOnClose(e) {
    }

    function socketOnOpen(e) {
    }

    function chatInit() {
        currentUser = Chat.currentUser;
        $('#msgWrapper').scroll(function () {
            if ($('#msgs').height() - $('#msgWrapper').height() - $('#msgWrapper').scrollTop() <= 50) {
                getEarlier();
            }
        });

        $('#txtContent').keypress(function (e) {
            if (e.ctrlKey && (e.keyCode == 10 || e.keCode == 13)) {
                chatPost();
                return false;
            }
        });

        getCommon(new Date, INITIAL_MSG_COUNT, function (data) {
            if (data.length) {
                latest = new Date(data[0].Time);
                earliest = new Date(data[data.length - 1].Time);

                data = data.map(msg2Div);
                $('#msgs').html($('<div style="text-align:center; color:gray;"/>')
                    .text('以下是历史信息'))
                data.forEach(function (line) {
                    $('#msgs').append(line);
                });
            }
        });

        if (Chat.currentUser) {
            socket = new WebSocket(Chat.socketURL);
            socket.onopen = socketOnOpen;
            socket.onmessage = socketOnMessage;
            socket.onerror = socketOnError;
            socket.onclose = socketOnClose;
        }
    }
    window.Chat = {
        currentUser: '',
        socketURL:'',
        init: chatInit,
        post: chatPost,
    };
})();