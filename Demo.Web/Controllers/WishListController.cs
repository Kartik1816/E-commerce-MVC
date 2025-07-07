using System.Text.Json;
using Demo.Web.Middleware;
using Demo.Web.Models;
using Demo.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Demo.Web.Controllers;

[JwtMiddleware]
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
        ResponseModel? responseModel = JsonConvert.DeserializeObject<ResponseModel>(jsonString);
        List<ProductViewModel>? products = JsonConvert.DeserializeObject<List<ProductViewModel>>(responseModel?.Data?.ToString() ?? string.Empty);
        WishListViewModel productListViewModel = new()
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
}
