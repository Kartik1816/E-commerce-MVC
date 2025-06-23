using System.Text.Json;
using Demo.Web.Middleware;
using Demo.Web.Models;
using Demo.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Demo.Web.Controllers;

[JwtMiddleware]
public class CartController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl = "http://localhost:5114/api/Cart/";

    public CartController(IHttpClientFactory httpClientFactory)
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

        HttpResponseMessage response = await _httpClient.GetAsync(_apiBaseUrl + "cartProducts/" + userId);
        string jsonString = await response.Content.ReadAsStringAsync();
        JsonDocument jsonObject = JsonDocument.Parse(jsonString);
        JsonElement dataObject = jsonObject.RootElement.GetProperty("data");
        List<CartProductViewModel>? products = System.Text.Json.JsonSerializer.Deserialize<List<CartProductViewModel>>(
            dataObject.ToString(),
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
        );
        CartViewModel productListViewModel = new()
        {
            CartProductViewModels = products,
        };
        return View(productListViewModel);
    }

    [HttpPost]
    [Route("/Cart/AddProductToCart")]
    public async Task<IActionResult> AddProductToCart(int productId)
    {
        string? token = Request.Cookies["token"];
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Index", "Auth");
        }

        int userId = JwtService.GetUserIdFromJwtToken(token);
        CartModel cartModel = new()
        {
            ProductId = productId,
            UserId = userId,
            Quantity = 1
        };

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(_apiBaseUrl + "add", cartModel);
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
        return StatusCode((int)response.StatusCode, "Error occurred while adding product to cart.");
    }

    [HttpPost]
    [Route("/Cart/RemoveProductFromCart")]
    public async Task<IActionResult> RemoveProductFromCart(int productId)
    {
        string? token = Request.Cookies["token"];
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Index", "Auth");
        }

        int userId = JwtService.GetUserIdFromJwtToken(token);
        CartModel cartModel = new()
        {
            ProductId = productId,
            UserId = userId,
        };

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(_apiBaseUrl + "remove", cartModel);
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
        return StatusCode((int)response.StatusCode, "Error occurred while removing product from cart.");
    }

    [HttpPost]
    public async Task<IActionResult> IncreaseQuantity(int productId)
    {
        if (productId <= 0)
        {
            return new JsonResult(new { success = false, message = "Product not found" });
        }
        string? token = Request.Cookies["token"];
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Index", "Auth");
        }
        int userId = JwtService.GetUserIdFromJwtToken(token);
        CartModel cartModel = new()
        {
            UserId = userId,
            ProductId = productId
        };
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(_apiBaseUrl + "increase-quantity", cartModel);
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

    [HttpPost]
    public async Task<IActionResult> DecreaseQuantity(int productId)
    {
        if (productId <= 0)
        {
            return new JsonResult(new { success = false, message = "Product not found" });
        }
        string? token = Request.Cookies["token"];
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Index", "Auth");
        }
        int userId = JwtService.GetUserIdFromJwtToken(token);
        CartModel cartModel = new()
        {
            UserId = userId,
            ProductId = productId
        };
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(_apiBaseUrl + "decrease-quantity", cartModel);
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
    public async Task<IActionResult> GetUpdatedCart()
    {
        string? token = Request.Cookies["token"];
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Index", "Auth");
        }

        int userId = JwtService.GetUserIdFromJwtToken(token);

        HttpResponseMessage response = await _httpClient.GetAsync(_apiBaseUrl + "cartProducts/" + userId);
        string jsonString = await response.Content.ReadAsStringAsync();
        JsonDocument jsonObject = JsonDocument.Parse(jsonString);
        JsonElement dataObject = jsonObject.RootElement.GetProperty("data");
        List<CartProductViewModel>? products = System.Text.Json.JsonSerializer.Deserialize<List<CartProductViewModel>>(
            dataObject.ToString(),
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
        );
        CartViewModel productListViewModel = new()
        {
            CartProductViewModels = products,
        };
        return PartialView("_CartProductList", productListViewModel);
    }
}
