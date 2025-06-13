let pass=true;
let confirmPass=true;
let OldPass=true;
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
function showOldPassword()
{
    var x = document.getElementById("oldpassword");
    if (OldPass) {
        x.type = "text";
        document.getElementById('showoldpassword').classList.add('d-none');
        document.getElementById('hideoldpassword').classList.remove('d-none');
        OldPass=false;

    } else {
        x.type = "password";
        document.getElementById('showoldpassword').classList.remove('d-none');
        document.getElementById('hideoldpassword').classList.add('d-none');
        OldPass=true;
    }
}

$('#changePasswordForm').submit(function (e) {
    e.preventDefault();
    if(!$(this).valid())
    {
        return;
    }
    var form=$(this)[0];
    var formData = new FormData(form);
    $.ajax({
        url: '/ChangePassword/UpdatePassword',
        type: 'POST',
        data: formData,
        contentType: false,
        processData: false,
        success: function (response) {
            if (response.success) {
                toastr.success(response.message);
                setTimeout(function () {
                    window.location.href = '/Home/Index';
                }, 1000);
            } else {
                toastr.error(response.message);
            }
        },
        error: function (xhr, status, error) {
            toastr.error("An error occurred while changing the password.");
        }
    });
});    