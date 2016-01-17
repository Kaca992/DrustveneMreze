$(':radio').change(
    function() {
        var rating = this.value;
        var id = $('#movieId').val();

        $('#userRatingRefresh').html(rating);

        $.ajax({
            type: "POST",
            url: '/Manage/UpdateUserRating',
            contentType: "application/json; charset=utf-8",
            data: "{ 'movieID': '" + id + "', 'rating': " + rating + "}",
            dataType: "json",
            success: function() {

            },
            error: function() { alert('Error'); }
        });

    }
);

$('#dislike').click(function() {

        var id = $('#movieId').val();
        $.ajax({
            type: "POST",
            url: '/Manage/DislikeMovie',
            contentType: "application/json; charset=utf-8",
            data: "{ 'movieID': '" + id + "'}",
            dataType: "json",
            success: function() {
                $('#movieIsLiked').hide();
                $('#movieIsNotLiked').show();
                $('#showRating').hide();
                $('#dntShowRating').show();
            },
            error: function() { alert('Error'); }
        });

    }
);

$('#like').click(function() {

        var id = $('#movieId').val();
        $.ajax({
            type: "POST",
            url: '/Manage/LikeMovie',
            contentType: "application/json; charset=utf-8",
            data: "{ 'movieID': '" + id + "'}",
            dataType: "json",
            success: function() {
                $('#movieIsLiked').show();
                $('#movieIsNotLiked').hide();
                $('#showRating').show();
                $('#dntShowRating').hide();
            },
            error: function() { alert('Error'); }
        });

    }
);

function loadLikeDislike() {
    alert("bok");
}