using System.Text.Json;
using Demo.Web.Models;
using Demo.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Demo.Web.Controllers;

public class OrdersController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl = "http://localhost:5114/api/Orders/";

    public OrdersController(IHttpClientFactory httpClientFactory)
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
        HttpResponseMessage response = await _httpClient.GetAsync(_apiBaseUrl + "get-user-orders/" + userId);
        string jsonString = await response.Content.ReadAsStringAsync();
        ResponseModel? responseModel = JsonConvert.DeserializeObject<ResponseModel>(jsonString);
        UserOrders? userOrders = JsonConvert.DeserializeObject<UserOrders>(responseModel?.Data.ToString() ?? string.Empty);
        
        return View(userOrders);
    }

    [HttpPost]
    public async Task<IActionResult> SaveCustomerReview(CustomerReviewModel customerReviewModel)
    {

        string? token = Request.Cookies["token"];
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Index", "Auth");
        }
        int userId = JwtService.GetUserIdFromJwtToken(token);
        customerReviewModel.UserId = userId;

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(_apiBaseUrl + "reviews", customerReviewModel);

        if (response.IsSuccessStatusCode)
        {
            string responseContent = response.Content.ReadAsStringAsync().Result;
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
