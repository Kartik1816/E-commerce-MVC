$('#editProfileForm').submit(function (e) {
    e.preventDefault(); 
    var form= $(this)[0];
   var formData = new FormData(form);
    $.ajax({
        type: 'POST',
        url: '/EditProfile/SaveProfile',
        data: formData,
        processData: false,
        contentType: false,
        success: function (response) {
            if (response.success) {
                toastr.success(response.message)
                setTimeout(()=> {
                    window.location.reload();
                }, 2000);
            } else {
                toastr.error(response.message);
            }
        },
        error: function () {
            alert('An error occurred while updating the profile.');
        }
    });
});