var encryptedCategoryId = window.location.pathname.split('/').pop();

function loadPage(pageNumber)
{
    console.log(categoryId);
    $.ajax({
        url: '/CLA/GetPaginatedProducts',
        data: { pageNo: pageNumber , encryptedCategoryId: encryptedCategoryId },
        type: "GET",
        success: function (response) {
            $('#productListContainer').html(response);
        },
        error: function () {
            alert("Error loading products.");
        }
    });
}