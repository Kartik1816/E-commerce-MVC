
//get Category Id from URL
var categoryId = window.location.pathname.split('/').pop();
$.ajax({
    type: 'GET',
    url: '/CLA/GetDecryptedId',
    data: { id: categoryId },
    success: function (data) {
        if (data.success) {
            categoryId=data.decryptedId;
            $('#categoryId').val(parseInt(categoryId));
            
            if(parseInt(categoryId) == 1)
            {
                $('#categoryName').text('Computers');
            }
            else if(parseInt(categoryId) == 2)
            {
                $('#categoryName').text('Laptops');
            }
            else
            {
                $('#categoryName').text('Accessories');
            }
        } else {
            toastr.error(data.message);
        }
    }
});



function SaveProduct()
{
    $('#addEditProductForm').off('click').submit(function (e) {
        e.preventDefault();
        if(!$(this).valid()) {
            return;
        }
        var form = $(this)[0];
        var formData = new FormData(form);
        $.ajax({
            type: 'POST',
            url: '/CLA/SaveProduct',
            data: formData,
            contentType: false,
            processData: false,
            success: function (data) {
                if (data.success) {
                    $('#addEditProduct').modal('hide');
                    toastr.success(data.message);
                    setTimeout(()=>{
                        window.location.reload();
                    },1000)
                } else {
                    toastr.error(data.message);
                }
            },
            error: function (xhr, status, error) {
                toastr.error('Error saving product: ' + error);
            }
        });
    });
}

function getProductDetails(productId)
{
    $.ajax({
        type: 'GET',
        url: '/CLA/GetProductDetails',
        data: {productId:productId},
        success: function(data)
        {
            $('#addEditProductView').html(data);
            $('#addEditProduct').modal('show');
            $.validator.unobtrusive.parse('#addEditProduct');
            SaveProduct();
        },
        error: function(data)
        {
            toastr.error("And Error occured while getting product details");
        }
    });
}
function resetProductModal()
{
    $.ajax({
        type: 'GET',
        url: '/CLA/ResetProductModal',
        success: function(data)
        {
            $('#addEditProductView').html(data);
            $('#addEditProduct').modal('show');
            $.validator.unobtrusive.parse('#addEditProduct');
            SaveProduct();
        },
        error: function(data)
        {
            toastr.error("And Error occured while getting product details");
        }
    });
}
function deleteProduct(productId)
{
    $('#deleteButton').click(function(){
        $.ajax({
            type: 'DELETE',
            url: '/CLA/DeleteProduct',
            data: {productId:productId},
            success: function (data) {
                if (data.success) {
                    toastr.success(data.message);
                    setTimeout(()=>{
                        window.location.reload();
                    },1000)
                } else {
                    toastr.error(data.message);
                }
            },
            error: function (xhr, status, error) {
                toastr.error('Error saving product: ' + error);
            }
        });
    });
}

function viewProductDetails(productId)
{
    $.ajax({
        type: 'GET',
        url: '/CLA/GetEncryptedId',
        data : { Id: productId },
        success: function (data) {
            if (data.success) {
                window.location.href=`/CLA/ViewProduct/${data.encryptedId}`;
            } else {
                toastr.error(data.message);
            }
        },
    });
    
}

