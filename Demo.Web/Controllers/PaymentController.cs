using Demo.Web.Models;
using Demo.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Demo.Web.Controllers;

public class PaymentController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl = "http://localhost:5114/api/Payment/";

    public PaymentController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
    }
    [HttpPost]
    public async Task<IActionResult> CreateOrder(string Amount)
    {
        string? token = Request.Cookies["token"];
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Index", "Auth");
        }
        int userId = JwtService.GetUserIdFromJwtToken(token);

        PaymentRequest paymentRequest = new()
        {
            Amount = Convert.ToDecimal(Amount),
            Currency = "INR",
            UserId = userId
        };
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(_apiBaseUrl + "create-order", paymentRequest);
        if (response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            dynamic? responseData = JsonConvert.DeserializeObject<dynamic>(responseContent);
            if (responseData != null)
            {
                dynamic orderId = responseData.orderId;
                int orderModelId = responseData.orderModelId;
                return Ok(new { orderId = orderId, orderModelId =  orderModelId});
            }
            else
            {
                return BadRequest(new { success = false, message = "Not matched url found" });
            }
        }
        else
        {
            return new JsonResult(new { success = false, message = "Invalid response from server" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> VerifyPayment(PaymentVerificationRequest request)
    {
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(_apiBaseUrl + "verify-payment", request);
        if (response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            dynamic? responseData = JsonConvert.DeserializeObject<dynamic>(responseContent);
            if (responseData != null)
            {
                return Ok(new { success = true, message =(string)responseData.message });
            }
            else
            {
                return BadRequest(new { success = false, message = "Not matched url found" });
            }
        }
        else
        {
            return new JsonResult(new { success = false, message = "Invalid response from server" });
        }
    }
}
