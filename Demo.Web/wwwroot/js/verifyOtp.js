$(document).ready(function () {
    var email = localStorage.getItem('forgotEmail');
    if (email) {
        $('#verify-otp-email').val(email);
    }
    localStorage.removeItem('forgotEmail');
});