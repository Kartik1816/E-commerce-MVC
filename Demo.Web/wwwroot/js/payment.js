
$("#rzp-button").off('click').click(function (e) {
    console.log($("#amount").val());
    if(!$("#amount").val() || parseFloat($("#amount").val())==0)
    {
        toastr.error("Please have valid amount to pay");
        return;
    }
    e.preventDefault();
    $.ajax({
        url: '/Payment/CreateOrder',
        type: 'POST',
        data: { Amount: parseFloat($("#amount").val()) },
        success: function (response) {
            launchRazorPay(response.orderId,response.orderModelId);
        }
    });
});

function launchRazorPay(orderId,orderModelId) {

    console.log(orderModelId);

    var options = {
        "key": "rzp_test_XoLf92aFuGEqXD", // RazorPay Key from MVC config
        "amount": parseFloat($("#amount").val()) * 100, // Amount in paise (100 INR)
        "currency": "INR",
        "name": "My E-Commerce",
        "description": "Test Payment",
        "order_id": orderId,
        "handler": function (response) {

            $.ajax({
                url: '/Payment/VerifyPayment',
                type: 'POST',
                data: {
                    PaymentId: response.razorpay_payment_id,
                    OrderId: response.razorpay_order_id,
                    Signature: response.razorpay_signature,
                    OrderModelId : parseInt(orderModelId)
                },
                success: function (response) {
                    toastr.success(response.message);
                    setTimeout(()=>{
                        window.location.href="/Home/Index";
                    },2000)
                }
            });
        },
        "prefill": {
            "name": "kartik",
            "email": "kartik@yopmail.com",
            "contact": "7436041801"
        },
        "theme": {
            "color": "#3399cc"
        }
    };

    var rzp = new Razorpay(options);
    rzp.open();
}
