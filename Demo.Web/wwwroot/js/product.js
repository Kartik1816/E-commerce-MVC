function addRemoveProductToFromWishList(productId)
{
    $.ajax({
        url: '/WishList/AddProductToWishList',
        type: 'POST',
        data: { productId: productId },
        success: function(response) {
            if (response.success) {
                toastr.success(response.message);
                setTimeout(()=>{
                    window.location.href = "/WishList/Index";
                },1000)
            } else {
                toastr.error(response.message);
            }
        },
        error: function() {
            toastr.error('An error occurred while adding the product to wishlist.');
        }
    });
}

function addProductToCart(productId) {
    $.ajax({
        url: '/Cart/AddProductToCart',
        type: 'POST',
        data: { productId: productId },
        success: function(response) {
            if (response.success) {
                toastr.success(response.message);
                setTimeout(()=>{
                    window.location.href="/Cart/Index";
                },1000)
            } else {
                toastr.error(response.message);
            }
        },
        error: function() {
            toastr.error('An error occurred while adding the product to the cart.');
        }
    });
}

function removeProductFromCart(productId) {
    $.ajax({
        url: '/Cart/RemoveProductFromCart',
        type: 'POST',
        data: { productId: productId },
        success: function(response) {
            if (response.success) {
                toastr.success(response.message);
                const path = window.location.pathname;
                if (path.includes("CLA")) {
                    setTimeout(()=>{
                        window.location.reload();
                    },1000)
                }
                else
                {
                    getUpdatedCart();
                }
                
            } else {
                toastr.error(response.message);
            }
        },
        error: function() {
            toastr.error('An error occurred while removing the product from the cart.');
        }
    });
}


function increaseQuantity(productId) {
    $.ajax({
        url: '/Cart/IncreaseQuantity',
        type: 'POST',
        data: { productId: productId },
        success: function(response) {
            if (response.success) {
                getUpdatedCart();
                toastr.success(response.message);
            } else {
                toastr.error(response.message);
            }
        },
        error: function() {
            toastr.error('An error occurred while increasing the product quantity.');
        }
    });
}

function decreaseQuantity(productId) {
    $.ajax({
        url: '/Cart/DecreaseQuantity',
        type: 'POST',
        data: { productId: productId },
        success: function(response) {
            if (response.success) {
                getUpdatedCart();
                toastr.success(response.message);
            } else {
                toastr.error(response.message);
            }
        },
        error: function() {
            toastr.error('An error occurred while decreasing the product quantity.');
        }
    });
}

function getUpdatedCart()
{
    $.ajax({
        url: '/Cart/GetUpdatedCart',
        type: 'GET',
        success: function(response) {
            $('#cartProductView').html(response);
        },
        error: function() {
            toastr.error('An error occurred while getting updated cart.');
        }
    });
}

function viewProductDetails(productId)
{
    window.location.href=`/CLA/ViewProduct/${productId}`;
}
