$(document).ready(function () {
    var email = localStorage.getItem('forgotEmail');
    if (email) {
        $('#verify-otp-email').val(email);
    }
});

$('#verify-otp-form').submit(function (e) {
    e.preventDefault(); 
    if(!$(this).valid()) {
        return; 
    }
    var form = $(this)[0];
    var formData = new FormData(form);
    
    console.log($('#verify-otp-email').val());
    
    $.ajax({
        url: '/Verify/VerifyOtp',
        method: 'POST',
        data: formData,
        contentType: false,
        processData: false,
        success: function (response) {
            if(response.success)
            {
                toastr.success(response.message);
                setTimeout(()=>{
                    window.location.href = '/ResetPassword/Index';
                },1000)
            }
            else
            {
                toastr.error(response.message);
            }
        },
        error: function (xhr, status, error) {
            toastr.error('An error occurred while processing your request. Please try again later.');
        }
    });
});