$(':radio').change(
  function () {
      var rating = this.value;
      var id = $('#movieId').val();
      
      
      $.ajax({
          type: "POST",
          url: '/Manage/UpdateUserRating',
          contentType: "application/json; charset=utf-8",
          data: "{ 'movieID': '" + id + "', 'rating': " + rating + "}",          
          dataType: "json",
          success: function () {
              
          },
          error: function () { alert('Error'); }
      });

  }
)