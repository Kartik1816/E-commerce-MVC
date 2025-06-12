
$('#forgot-password-form').submit(function (e) {
    e.preventDefault(); 
    if(!$(this).valid()) {
        return; 
    }
    var form = $(this)[0];
    var formData = new FormData(form);
        email= $('#forgot-email').val();
        console.log(email);
    localStorage.setItem('forgotEmail', email);
    debugger;
    $.ajax({
        url: '/ForgotPassword/ForgotPassword',
        type: 'POST',
        data: formData,
        contentType: false,
        processData: false,
        success: function (response) {
            if(response.success)
            {
                toastr.success(response.message);
                setTimeout(()=>{
                    window.location.href = '/ForgotPassword/Verify';
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

$('#verify-otp-form').submit(function (e) {
    e.preventDefault(); 
    if(!$(this).valid()) {
        return; 
    }
    var form = $(this)[0];
    var formData = new FormData(form);

    $.ajax({
        url: '/ForgotPassword/Verify',
        type: 'POST',
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

