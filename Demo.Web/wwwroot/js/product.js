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
                    window.location.reload();
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
                setTimeout(()=>{
                    window.location.reload();
                },1000)
            } else {
                toastr.error(response.message);
            }
        },
        error: function() {
            toastr.error('An error occurred while removing the product from the cart.');
        }
    });
}