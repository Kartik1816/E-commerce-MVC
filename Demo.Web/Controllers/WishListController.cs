using System.Text.Json;
using Demo.Web.Models;
using Demo.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Demo.Web.Controllers;

public class WishListController : Controller
{
    private readonly HttpClient _httpClient;

    private readonly string _apiBaseUrl = "http://localhost:5114/api/WishList/";

    public WishListController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task<IActionResult> Index()
    {
        string? token = Request.Cookies["token"];
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Index", "Auth");
        }
        int userId = JwtService.GetUserIdFromJwtToken(token);
        HttpResponseMessage response = await _httpClient.GetAsync(_apiBaseUrl + "wishListProducts/" + userId);
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
        WishListViewModel productListViewModel = new ()
        {
            Products = products ?? new List<ProductViewModel>(),
        };
        return View(productListViewModel);
    }

    [HttpPost]
    [Route("/WishList/AddProductToWishList")]
    public async Task<IActionResult> AddProductToWishList(int productId)
    {
        string? token = Request.Cookies["token"];
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Index", "Auth");
        }
        int userId = JwtService.GetUserIdFromJwtToken(token);
        WishListModel wishListModel = new()
        {
            ProductId = productId,
            UserId = userId
        };
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(_apiBaseUrl + "addRemoveToFromWishList", wishListModel);
        if (response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            dynamic? responseData = JsonConvert.DeserializeObject<dynamic>(responseContent);
            if (responseData != null)
            {
                string message = responseData.message;
                bool success = responseData.success;
                return new JsonResult(new { success = true, message = message });

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
}
