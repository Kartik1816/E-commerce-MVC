
//get Category Id from URL
var categoryId = window.location.pathname.split('/').pop();
$('#categoryId').val(parseInt(categoryId));

function SaveProduct()
{
    $('#addEditProductForm').submit(function (e) {
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