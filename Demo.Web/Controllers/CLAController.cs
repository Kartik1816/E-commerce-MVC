using System.Text.Json;
using Demo.Web.Models;
using Demo.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Demo.Web.Controllers;

public class CLAController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl = "http://localhost:5114/api/CLA/";
    public CLAController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
    }

    [HttpGet]
    [Route("CLA/{categoryId}")]
    public async Task<IActionResult> Index(int categoryId)
    {
        HttpResponseMessage response = await _httpClient.GetAsync(_apiBaseUrl + "products/" + categoryId);
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
    public async Task<IActionResult> SaveProduct([FromForm] ProductViewModel productViewModel)
    {
        if (!ModelState.IsValid)
        {
            return new JsonResult(new { success = false, message = "Please Enter Valid data" });
        }
        string? token = Request.Cookies["token"];
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Index", "Auth");
        }
        int userId = JwtService.GetUserIdFromJwtToken(token);
        productViewModel.UserId = userId;

        if (productViewModel.ProductImage != null)
        {
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(productViewModel.ProductImage.FileName);
            if (!productViewModel.ProductImage.ContentType.Contains("image"))
            {
                return new JsonResult(new { success = false, message = "Please upload a valid image file." });
            }
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/product-images", fileName);
            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                productViewModel.ProductImage.CopyTo(fileStream);
            }
            productViewModel.ImageUrl = fileName;
        }
        //temporarily remove Image of type IFormFile from view model then send
        productViewModel.ProductImage = null;

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(_apiBaseUrl + "saveProduct", productViewModel);
        if (response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            dynamic? responseData = JsonConvert.DeserializeObject<dynamic>(responseContent);
            if (responseData != null)
            {
                if (!responseData.success)
                {
                    //remove the image if registration fails
                    if (productViewModel.ImageUrl != null)
                    {
                        string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/product-images", productViewModel.ImageUrl);
                        if (System.IO.File.Exists(path))
                        {
                            System.IO.File.Delete(path);
                        }
                    }
                }
                string message = responseData.message;
                bool success = responseData.success;
                return new JsonResult(new { success = success, message = message });
            }
            else
            {
                if (productViewModel.ImageUrl != null)
                {
                    string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/product-images", productViewModel.ImageUrl);
                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }
                }
                return new JsonResult(new { success = false, message = "Invalid response from server." });
            }
        }
        else
        {
            return StatusCode((int)response.StatusCode, "Error occurred while processing the request.");
        }
    }
}
