using System.Text.Json;
using Demo.Web.Middleware;
using Demo.Web.Models;
using Demo.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;

namespace Demo.Web.Controllers;


public class CLAController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl = "http://localhost:5114/api/CLA/";
    private readonly EmailService _emailService;
    public CLAController(IHttpClientFactory httpClientFactory, EmailService emailService)
    {
        _httpClient = httpClientFactory.CreateClient();
        _emailService = emailService;
    }

    [HttpGet]
    [Route("CLA/{encryptedCategoryId}")]
    public async Task<IActionResult> Index(string encryptedCategoryId, int pageNumber = 1, int pageSize = 10)
    {
        if (string.IsNullOrEmpty(encryptedCategoryId))
        {
            return RedirectToAction("Index", "Home");
        }
        int categoryId = EncryptDecryptService.DecryptId(encryptedCategoryId);

        string? token = Request.Cookies["token"];
        int userId = 0;
        if (token != null)
        {
            userId = JwtService.GetUserIdFromJwtToken(token);
        }
        string queryString = $"?CategoryId={categoryId}&UserId={userId}&PageNumber={pageNumber}&PageSize={pageSize}";
        HttpResponseMessage response = await _httpClient.GetAsync(_apiBaseUrl + "products/" + queryString);
        string jsonString = await response.Content.ReadAsStringAsync();
        ResponseModel? dataObject = JsonConvert.DeserializeObject<ResponseModel>(jsonString);
        PaginatedResponse<ProductViewModel>? paginatedResponse = dataObject?.Data != null
            ? JsonConvert.DeserializeObject<PaginatedResponse<ProductViewModel>>(dataObject.Data.ToString())
            : new PaginatedResponse<ProductViewModel>();

        List<ProductViewModel>? products = paginatedResponse?.Data ?? new List<ProductViewModel>();
        CLAViewModel productListViewModel = new CLAViewModel
        {
            Products = products ?? new List<ProductViewModel>(),
            PageNumber = paginatedResponse?.PageNumber ?? 1,
            PageSize = paginatedResponse?.PageSize ?? 10,
            TotalRecords = paginatedResponse?.TotalRecords ?? 0,
        };
        return View(productListViewModel);
    }

    [HttpPost]
    [JwtMiddleware]
    public async Task<IActionResult> SaveProduct([FromForm] ProductViewModel productViewModel)
    {
        if (!ModelState.IsValid)
        {
            return new JsonResult(new { success = false, message = "Please Enter Valid data" });
        }
        if (productViewModel.Id <= 0 && productViewModel.ProductImages == null)
        {
            return new JsonResult(new { success = false, message = "Image can not be null for new Product" });
        }
        string? token = Request.Cookies["token"];
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Index", "Auth");
        }
        int userId = JwtService.GetUserIdFromJwtToken(token);
        productViewModel.UserId = userId;

        List<string> imageUrls = new List<string>();

        if (productViewModel.ProductImages != null && productViewModel.ProductImages.Count > 0)
        {
            foreach (IFormFile image in productViewModel.ProductImages)
            {
                if (!image.ContentType.Contains("image"))
                {
                    return new JsonResult(new { success = false, message = "Please upload valid image files." });
                }

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/product-images", fileName);

                using (FileStream fileStream = new FileStream(path, FileMode.Create))
                {
                    image.CopyTo(fileStream);
                }

                imageUrls.Add(fileName);
            }
        }
        productViewModel.ProductImages = null;
        productViewModel.ImageUrls = imageUrls;
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(_apiBaseUrl + "saveProduct", productViewModel);
        if (response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            ResponseModel? responseData = JsonConvert.DeserializeObject<ResponseModel>(responseContent);
            if (responseData != null)
            {
                string? message = responseData.Message;
                bool success = responseData.IsSuccess;

                ProductOfferModel? productOfferModel = responseData.Data != null
                    ? JsonConvert.DeserializeObject<ProductOfferModel>(responseData.Data.ToString())
                    : null;

                bool offer = productOfferModel?.IsOffer ?? false;

                if (offer)
                {
                    HttpResponseMessage userData = await _httpClient.GetAsync(_apiBaseUrl + "GetAllSubscribedUsers");
                    string jsonString = await userData.Content.ReadAsStringAsync();
                    SubscribedUsersModel? subscribedUsersModel = JsonConvert.DeserializeObject<SubscribedUsersModel>(jsonString);

                    ProductViewModel? discountedProduct = productOfferModel?.ProductViewModel;
                    if (subscribedUsersModel == null || discountedProduct == null)
                    {
                        return new JsonResult(new { success = false, message = "Failed to retrieve subscribed users or product details." });
                    }
                    if (subscribedUsersModel != null && discountedProduct != null && subscribedUsersModel.SubscribedUsers != null && subscribedUsersModel.SubscribedUsers.Count > 0)
                    {
                        try
                        {
                            await _emailService.OfferMailToAll(subscribedUsersModel.SubscribedUsers, discountedProduct);
                        }
                        catch (Exception e)
                        {
                            throw new Exception("An Exception occured while sending mail to all subscribed users" + e);
                        }
                    }
                }
                return new JsonResult(new { success = success, message = message });
            }
            else
            {
                return new JsonResult(new { success = false, message = "Invalid response from server." });
            }
        }
        else
        {
            return StatusCode((int)response.StatusCode, "Error occurred while processing the request.");
        }
    }

    [HttpGet]
    [Route("/CLA/GetProductDetails")]
    [JwtMiddleware]
    public async Task<IActionResult> GetProductDetails(int productId)
    {
        HttpResponseMessage response = await _httpClient.GetAsync(_apiBaseUrl + productId);
        string jsonString = await response.Content.ReadAsStringAsync();
        ResponseModel? responseData = JsonConvert.DeserializeObject<ResponseModel>(jsonString);
        ProductViewModel? productDetails = JsonConvert.DeserializeObject<ProductViewModel>(responseData?.Data?.ToString() ?? string.Empty);

        CLAViewModel cLAViewModel = new()
        {
            productViewModel = productDetails
        };
        return PartialView("_AddEditProduct", cLAViewModel);
    }

    [HttpGet]
    [Route("/CLA/ResetProductModal")]
    [JwtMiddleware]
    public IActionResult ResetProductModal()
    {
        CLAViewModel cLAViewModel = new() { };
        return PartialView("_AddEditProduct", cLAViewModel);
    }

    [HttpDelete]
    [Route("/CLA/DeleteProduct")]
    [JwtMiddleware]
    public async Task<IActionResult> DeleteProduct(int productId)
    {
        if (productId <= 0)
        {
            return new JsonResult(new { success = false, message = "Product not found" });
        }
        HttpResponseMessage response = await _httpClient.DeleteAsync(_apiBaseUrl + productId);
        if (response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            ResponseModel? responseData = JsonConvert.DeserializeObject<ResponseModel>(responseContent);
            if (responseData != null)
            {
                string? message = responseData.Message;
                bool success = responseData.IsSuccess;
                return new JsonResult(new { success = success, message = message });

            }
            else
            {
                return new JsonResult(new { success = false, message = "Invalid response from server." });
            }
        }
        else
        {
            return StatusCode((int)response.StatusCode, "Error occurred while processing the request.");
        }
    }

    [HttpGet]
    [Route("/CLA/ViewProduct/{encryptedProductId}")]
    [JwtMiddleware]
    public async Task<IActionResult> ProductDetails(string encryptedProductId)
    {
        string? token = Request.Cookies["token"];
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Index", "Auth");
        }
        int productId = EncryptDecryptService.DecryptId(encryptedProductId);
        if (productId <= 0)
        {
            return RedirectToAction("Index", "Home");
        }
        int userId = JwtService.GetUserIdFromJwtToken(token);
        string queryString = $"?ProductId={productId}&UserId={userId}";
        HttpResponseMessage response = await _httpClient.GetAsync(_apiBaseUrl + "GetProductDetails/" + queryString);
        string jsonString = await response.Content.ReadAsStringAsync();
        ResponseModel? responseData = JsonConvert.DeserializeObject<ResponseModel>(jsonString);
        ProductViewModel? productDetails = JsonConvert.DeserializeObject<ProductViewModel>(responseData?.Data?.ToString() ?? string.Empty);
        if (productDetails == null)
        {
            return RedirectToAction("Index", "Home");
        }
        return View(productDetails);
    }

    [HttpGet]
    [Route("/CLA/GetEncryptedId")]
    public IActionResult GetEncryptedId(int Id)
    {
        if (Id <= 0)
        {
            return new JsonResult(new { success = false, message = "Invalid ID" });
        }
        string encryptedId = EncryptDecryptService.EncryptId(Id);

        return new JsonResult(new { success = true, encryptedId = encryptedId });
    }

    [HttpGet]
    [Route("/CLA/GetPaginatedProducts")]
    public async Task<IActionResult> GetPaginatedProducts(int pageNo, string encryptedCategoryId)
    {
        string? token = Request.Cookies["token"];

        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Index", "Auth");
        }

        int userId = JwtService.GetUserIdFromJwtToken(token);

        PaginationRequestModel paginationRequestModel = new PaginationRequestModel
        {
            PageNumber = pageNo,
            PageSize = 10
        };

        if (string.IsNullOrEmpty(encryptedCategoryId))
        {
            return RedirectToAction("Index", "Home");
        }

        int categoryId = EncryptDecryptService.DecryptId(encryptedCategoryId);

        if (categoryId <= 0)
        {
            return RedirectToAction("Index", "Home");
        }

        string queryString = $"?CategoryId={categoryId}&UserId={userId}&PageNumber={paginationRequestModel.PageNumber}&PageSize={paginationRequestModel.PageSize}";
        HttpResponseMessage response = await _httpClient.GetAsync(_apiBaseUrl + "products/" + queryString);

        string jsonString = response.Content.ReadAsStringAsync().Result;

        ResponseModel? dataObject = JsonConvert.DeserializeObject<ResponseModel>(jsonString);

        PaginatedResponse<ProductViewModel>? paginatedResponse = dataObject?.Data != null
            ? JsonConvert.DeserializeObject<PaginatedResponse<ProductViewModel>>(dataObject.Data.ToString())
            : new PaginatedResponse<ProductViewModel>();

        List<ProductViewModel>? products = paginatedResponse?.Data ?? new List<ProductViewModel>();

        CLAViewModel productListViewModel = new CLAViewModel
        {
            Products = products ?? new List<ProductViewModel>(),
            PageNumber = paginatedResponse?.PageNumber ?? 1,
            PageSize = paginatedResponse?.PageSize ?? 10,
            TotalRecords = paginatedResponse?.TotalRecords ?? 0,
        };

        return PartialView("_ProductList", productListViewModel);
    }

}
