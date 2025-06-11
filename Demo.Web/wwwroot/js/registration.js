let pass=true;
let confirmPass=true;
function showPasswordConfirm() {
    var x = document.getElementById("confirm-password");
    if (confirmPass) {
        x.type = "text";
        document.getElementById('confirm-showpassword').classList.add('d-none');
        document.getElementById('confirm-hidepassword').classList.remove('d-none');
        confirmPass=false;

    } else {
        x.type = "password";
        document.getElementById('confirm-showpassword').classList.remove('d-none');
        document.getElementById('confirm-hidepassword').classList.add('d-none');
        confirmPass=true;
    }
}


function showPassword() {
  console.log('showPassword');
    var x = document.getElementById("password");
    if (pass) {
        x.type = "text";
        document.getElementById('showpassword').classList.add('d-none');
        document.getElementById('hidepassword').classList.remove('d-none');
        pass=false;

    } else {
        x.type = "password";
        document.getElementById('showpassword').classList.remove('d-none');
        document.getElementById('hidepassword').classList.add('d-none');
        pass=true;
    }
}

$('#confirm-password').on('input', function () {
    validatePasswordAndConfirmPassword();
    $('#confirmation-password-error').text('');
  });
 function validatePasswordAndConfirmPassword()
 {
    var password = $('#password').val();
    var confirmPassword = $('#confirm-password').val();
    if (password !== confirmPassword) {
        $('#confirmation-password-error').text('Passwords do not match');
    } else {
        $('#confirmation-password-error').text('');
    }
 }
$('#registration-form').submit(function (e) {
    e.preventDefault();
    if(!$(this).valid()) {
        return;
    }
    var form = $(this)[0];
    var formData = new FormData(form);
    $.ajax({
        type: 'POST',
        url: '/Auth/Registration',
        data: formData,
        processData: false,
        contentType: false,
        success: function (response) {
            if (response.success) {
                toastr.success(response.message);
                setTimeout(()=>{
                    window.location.href = '/Auth/Index';
                },2000)
            } else {
                toastr.error(response.message);
            }
        },
        error: function (xhr, status, error) {
            toastr.error('An error occurred while processing your request.');
        }
    });
});