
    document.getElementById("rzp-button").onclick = function (e) {
        e.preventDefault();

        // Call your MVC Controller (which will call Web API)
        $.ajax({
            url: '/Payment/CreateOrder',
            type: 'POST',
            data: JSON.stringify({ Amount: 100 }), // Replace with actual amount
            contentType: 'application/json',
            success: function (response) {
                launchRazorPay(response.orderId);
            }
        });
    };

    function launchRazorPay(orderId) {
        var options = {
            "key": "rzp_test_XoLf92aFuGEqXD", // RazorPay Key from MVC config
            "amount": 100 * 100, // Amount in paise (100 INR)
            "currency": "INR",
            "name": "My E-Commerce",
            "description": "Test Payment",
            "order_id": orderId,
            "handler": function (response) {
                // Handle successful payment
                alert("Payment successful! Payment ID: " + response.razorpay_payment_id);
                
                // Verify payment (optional, call your backend)
                $.post('@Url.Action("VerifyPayment", "Payment")', {
                    razorpayPaymentId: response.razorpay_payment_id,
                    razorpayOrderId: response.razorpay_order_id,
                    razorpaySignature: response.razorpay_signature
                });
            },
            "prefill": {
                "name": "Customer Name",
                "email": "customer@example.com",
                "contact": "9999999999"
            },
            "theme": {
                "color": "#3399cc"
            }
        };

        var rzp = new Razorpay(options);
        rzp.open();
    }
