
$('#subscribeForm').off("submit").submit(function(e){
        console.log("aslkdfjasdlkfj");
        e.preventDefault();
        var email =  $('#subscriber-email').val()
        var isEmailValid = validateEmail(email);
        if(isEmailValid)
        {
            $.ajax({
                url: '/Home/SubscribeNewUser',
                type: 'POST',
                data: {email:email},
                success: function (data) {
                if (data.success)
                {
                    toastr.success(data.message)
                    setTimeout(()=>{
                        window.location.reload();
                    },1000);
                }
                else 
                {
                    toastr.error(data.message);
                }
                },
                error: function (err) 
                {
                    toastr.error('An error occurred while processing your request');
                }
            });
        }
});

function validateEmail (email) {
    if (email.length <= 0) {
        $('#email-error').text('Email is required');
        return false;
    } else if (!email.match(/^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/)) {
        $('#email-error').text('Email is invalid');
        return false;
    } else {
      $('#email-error').text('');
       return true;
    }
}
