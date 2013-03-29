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

    function getOnlineList(callback) {
        $.getJSON('/api/WebSocket?onlineList=', callback);
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
                            .css('color', msg.UserColor)
                            .attr('title', msg.UserCaption)
                            .attr('href', '/User/Show/' + msg.User)
                            .text(msg.User))
                        .append(' ')
                        .append(new Date(msg.Time).toLocaleString()))
                    .append($('<div/>')
                        .append($('<pre style="display:inline-block; border:none; text-align:left;"/>').text(msg.Content))))
                .append('<div style="clear: both;"/>');
        } else {
            return $('<div class="inner"/>')
                .append($('<div class="image-inner right"/>')
                    .append($('<img alt="" style="width:50px; height:50px;"/>')
                        .attr('src', msg.UserImg)))
                .append($('<fieldset class="fieldset-inner right-text"/>')
                    .append($('<legend class="legend-inner right-text"/>')
                        .append($('<a/>')
                            .css('color', msg.UserColor)
                            .attr('title', msg.UserCaption)
                            .attr('href', '/User/Show/' + msg.User)
                            .text(msg.User))
                        .append(' ')
                        .append(new Date(msg.Time).toLocaleString()))
                    .append($('<div/>')
                        .append($('<pre style="display:inline-block; border:none; text-align:left;"/>').text(msg.Content))))
                .append('<div style="clear: both;"/>');
        }
    }

    function getEarlier() {
        if (noEarlier || gettingEarlier) {
            return;
        }
        $('#msgs').append('<progress style="width:100%;"/>');
        gettingEarlier = true;
        getCommon(earliest, EACH_SCROLL_COUNT, function (data) {
            if (EACH_SCROLL_COUNT != data.length)
                noEarlier = true;
            $('#msgs progress').remove();
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
        if (!$('#txtContent').val().trim()) return;
        postCommon($('#txtContent').val());
        $('#txtContent').attr('disabled', true);
    }

    function socketOnMessage(e) {
        var msg = JSON.parse(e.data);
        switch (msg.Type) {
            case 'CommonMessage':
                var div = msg2Div(msg.Message);
                $('#msgs').prepend(div);
                div.hide().fadeIn();
                break;
            case 'Login':
                $('#lstOnline').append(user2online(msg));
                break;
            case 'Logout':
                $('#lstOnline tr').filter(function () { return $(this).attr('data-user') == msg.User; }).remove();
                break;
        }
    }

    function socketOnError(e) {
    }

    function socketOnClose(e) {
    }

    function socketOnOpen(e) {
    }

    function user2online(user) {
        if (user == currentUser) {
            return $('<tr/>')
                    .attr('data-user', user.User)
                    .append($('<td/>')
                        .append('<img src="/Image/Myself.png" style="height:16px; width:16px; margin-right:2px;"/>')
                        .append($('<a/>')
                            .text(user.User)
                            .attr('title', user.UserCaption)
                            .css('color', user.UserColor)
                            .attr('href', '/User/Show/' + user)));
        } else {
            return $('<tr/>')
                    .attr('data-user', user.User)
                    .append($('<td/>')
                        .append('<img src="/Image/Online.png" style="height:16px; width:16px; margin-right:2px;"/>')
                        .append($('<a/>')
                            .text(user.User)
                            .attr('title', user.UserCaption)
                            .css('color', user.UserColor)
                            .attr('href', '/User/Show/' + user)));
        }
    }

    function chatInit() {
        currentUser = Chat.currentUser;
        $('#msgWrapper').scroll(function () {
            if ($('#msgs').height() - $('#msgWrapper').height() - $('#msgWrapper').scrollTop() <= 50) {
                getEarlier();
            }
        });

        $('#txtContent').keypress(function (e) {
            if (e.ctrlKey && (e.keyCode == 10 || e.keyCode == 13)) {
                chatPost();
                return false;
            }
        });

        $('#msgs').append('<progress style="width:100%;"/>');
        getCommon(new Date, INITIAL_MSG_COUNT, function (data) {
            $('#msgs progress').remove();
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

        getOnlineList(function (list) {
            list = list.map(user2online);
            list.forEach(function (div) {
                $('#lstOnline').append(div);
            });
        });

        if (Chat.currentUser) {
            socket = new WebSocket(Chat.socketURL + '?forceLogin=true');
            socket.onopen = socketOnOpen;
            socket.onmessage = socketOnMessage;
            socket.onerror = socketOnError;
            socket.onclose = socketOnClose;
        }
    }
    window.Chat = {
        currentUser: '',
        socketURL: '',
        init: chatInit,
        post: chatPost,
    };
})();