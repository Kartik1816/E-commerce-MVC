using System.Text.Json;
using Demo.Web.Middleware;
using Demo.Web.Models;
using Demo.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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
    [Route("CLA/{categoryId}")]
    public async Task<IActionResult> Index(int categoryId)
    {
         string? token = Request.Cookies["token"];
        int userId = 0;
        if (token != null)
        {
            userId = JwtService.GetUserIdFromJwtToken(token);
        }
        string queryString = $"?CategoryId={categoryId}&UserId={userId}";
        HttpResponseMessage response = await _httpClient.GetAsync(_apiBaseUrl + "products/" + queryString);
        string jsonString = await response.Content.ReadAsStringAsync();
        JsonDocument jsonObject = JsonDocument.Parse(jsonString);
        JsonElement dataObject = jsonObject.RootElement.GetProperty("data");
        List<ProductViewModel>? products = System.Text.Json.JsonSerializer.Deserialize<List<ProductViewModel>>(
            dataObject.ToString(),
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
        );
        CLAViewModel productListViewModel = new CLAViewModel
        {
            Products = products ?? new List<ProductViewModel>(),
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
        if (productViewModel.Id <= 0 && productViewModel.ProductImage == null)
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
            dynamic? responseData = JsonConvert.DeserializeObject<dynamic>(responseContent);
            if (responseData != null)
            {
                string message = responseData.message;
                bool success = responseData.success;
                bool offer = responseData.offer ?? false;
                if (offer)
                {
                    HttpResponseMessage userData = await _httpClient.GetAsync(_apiBaseUrl + "GetAllSubscribedUsers");
                    string jsonString = await userData.Content.ReadAsStringAsync();
                    JsonDocument jsonObject = JsonDocument.Parse(jsonString);
                    JsonElement dataObject = jsonObject.RootElement.GetProperty("data");
                    SubscribedUsersModel? subscribedUsersModel = System.Text.Json.JsonSerializer.Deserialize<SubscribedUsersModel>(
                        dataObject.ToString(),
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }
                    );

                    JsonDocument jsonObjectOfDiscountedProduct = JsonDocument.Parse(responseContent);
                    JsonElement productObject = jsonObjectOfDiscountedProduct.RootElement.GetProperty("data");
                    ProductViewModel? discountedProduct = System.Text.Json.JsonSerializer.Deserialize<ProductViewModel>(
                        productObject.ToString(),
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }
                    );

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
        JsonDocument jsonObject = JsonDocument.Parse(jsonString);
        JsonElement dataObject = jsonObject.RootElement.GetProperty("data");
        ProductViewModel? productDetails = System.Text.Json.JsonSerializer.Deserialize<ProductViewModel>(
            dataObject.ToString(),
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
        );

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
            dynamic? responseData = JsonConvert.DeserializeObject<dynamic>(responseContent);
            if (responseData != null)
            {
                string message = responseData.message;
                bool success = responseData.success;
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
    [Route("/CLA/ViewProduct/{productId}")]
    [JwtMiddleware]
    public async Task<IActionResult> ProductDetails(int productId)
    {
        string? token = Request.Cookies["token"];
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Index", "Auth");
        }
        int userId = JwtService.GetUserIdFromJwtToken(token);
        string queryString = $"?ProductId={productId}&UserId={userId}";
        HttpResponseMessage response = await _httpClient.GetAsync(_apiBaseUrl + "GetProductDetails/" + queryString);
        string jsonString = await response.Content.ReadAsStringAsync();
        JsonDocument jsonObject = JsonDocument.Parse(jsonString);
        JsonElement dataObject = jsonObject.RootElement.GetProperty("data");
        ProductViewModel? productDetails = System.Text.Json.JsonSerializer.Deserialize<ProductViewModel>(
            dataObject.ToString(),
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
        );
        return View(productDetails);
    }
}
