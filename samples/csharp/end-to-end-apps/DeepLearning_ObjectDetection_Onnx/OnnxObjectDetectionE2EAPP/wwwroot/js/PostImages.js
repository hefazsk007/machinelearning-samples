


$(function ()
{
    function LoadImagesFromServer() {
        $.ajax({
            url: "/api/Values",
            type: "GET",
            success: function (result) {
                $.each(result, function (index, value) {  
                    var data = result[index].encodedImageString;
                    var id = result[index].imageFileName;
                    var img = $('<img>'); //Equivalent: $(document.createElement('img'))
                    img.attr('src', 'data:image/jpeg;base64,' + data);
                    img.attr('height', 100);
                    img.attr('width', 100);
                    img.attr('id', id);
                    img.attr('class', 'input-image');
                    $("<li>", { html: img }).appendTo("#imagesUL"); 
                    $("#resultImagediv").css('margin-top', '20px'); 
                    $("#resultImagediv").css('background', 'lightgray'); 
                })
            

            },
            error: function (e) {
                var x = e;
            }
        });
    }
    LoadImagesFromServer();
   
    $('#imagesUL').on('click', 'img', function () {
        
        var datatoPost = $(this).attr("id");

        $.ajax({
            url: "/api/ObjectDetection/Identify",
            type: "POST",
            data: JSON.stringify(datatoPost),
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',            
            success: function (result) {
                $("#resultImagediv").css('background', 'none'); 
                var data = result.imageString;
                $("#result").attr("src", 'data:image/jpeg;base64,' + data);

            },
            error: function (e) {
                var x = e;
            }
        });
    });
        
    $(".input-image").click(function (e) {
        $(".input-image").removeClass("active");
        $(this).addClass("active");

        var url = $(this).attr("src");

        $.ajax({
            url: "/api/ObjectDetection?url=" + url,
            type: "GET",
            success: function (result) {
                var data = result.imageString;
                $("#result").attr("src", 'data:image/jpeg;base64,' + data);

            },
            error: function (e) {
                var x = e;
            }
        });
    });
  }
);

var form = document.querySelector('form');

form.addEventListener('submit', e => {
    e.preventDefault();

    //alert('Before image submit');

    const files = document.querySelector('[type=file]').files;

    const formData = new FormData();

    if (files.length == 0) { alert("Select an image to upload"); }

    else {
        formData.append('imageFile', files[0]);

        // Sending the image data to Server
        $.ajax({
            type: 'POST',
            url: '/api/ObjectDetection/IdentifyObjects',
            data: formData,
            contentType: false,
            processData: false,
            success: function (result) {
                var data = result.imageString;
                $("#result").attr("src", 'data:image/jpeg;base64,' + data);
            }
        });
    }

});
