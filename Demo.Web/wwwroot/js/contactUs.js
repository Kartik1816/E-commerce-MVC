$('#contactUsForm').submit(function (e) {
    e.preventDefault(); 
    if(!$(this).valid()){
        return; 
    }
    var form=$(this)[0];
    var formData = new FormData(form);
    $.ajax({
        url: '/ContactUs/Contact',
        type: 'POST',
        data: formData,
        contentType: false,
        processData: false,
        success: function (response) {
            if(response.success) {
                toastr.success(response.message);
                setTimeout(() => {
                    window.location.href = '/Home/Index';
                }, 1000);
            } else {
                toastr.error(response.message);
            }
        },
        error: function (xhr, status, error) {
            toastr.error('An error occurred while processing your request. Please try again later.');
        }
    });
});