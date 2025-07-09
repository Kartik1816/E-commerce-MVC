var categoryId = window.location.pathname.split('/').pop();
function loadPage(pageNumber)
{
    $.ajax({
        url: '/CLA/GetPaginatedProducts',
        data: { pageNo: pageNumber , encryptedCategoryId: categoryId },
        type: "GET",
        success: function (response) {
            $('#productListContainer').html(response);
        },
        error: function () {
            alert("Error loading products.");
        }
    });
}