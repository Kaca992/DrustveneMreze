$('#fill').click(function () {
    $('#fillFeed').html('');
    var pages = parseInt($('#fillAmount').val());
    var start = parseInt($('#fillStart').val());
    if (!isNaN(pages) && !isNaN(start)) {
        $('#fill').prop('disabled', true);
        doAjax(start, pages + start - 1);
    }
});

function doAjax(page, pageMax) {
    if (page > pageMax) {
        $('#fill').prop('disabled', false);
        return;
    }
    else {
        $('#fillFeed').append('Getting page ' + page + ' of ' + pageMax + '<br>');
        $.ajax({
            type: "GET",
            url: '/Home/FillMovie?page=' + page,
            dataType: "json",
            success: function (data) {
                console.log(data);
                if (data == 'ok') {
                    $('#fillFeed').append('Got page ' + page + ' of ' + pageMax + '<br>');
                    doAjax(page + 1, pageMax);
                }
                else {
                    $('#fill').prop('disabled', 'false');
                    alert('Error');
                }
            },
            error: function (data) {
                console.log(data);
                if (data.responseText == 'ok') {
                    $('#fillFeed').append('Got page ' + page + ' of ' + pageMax + '<br>');
                    doAjax(page + 1, pageMax);
                }
                else {
                    $('#fill').prop('disabled', 'false');
                    alert('Error');
                }
            },
            timeout: 300000
        });
    }
}