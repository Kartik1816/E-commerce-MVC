using Demo.Web.Models;
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
        PaymentRequest paymentRequest = new()
        {
            Amount = Convert.ToDecimal(Amount),
            Currency = "INR"
        };
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(_apiBaseUrl + "create-order", paymentRequest);
        if (response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            dynamic? responseData = JsonConvert.DeserializeObject<dynamic>(responseContent);
            if (responseData != null)
            {
                dynamic orderId = responseData.orderId;
                return Ok(new { orderId = orderId });
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
