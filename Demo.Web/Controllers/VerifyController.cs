using System.Net.Http;
using Demo.Web.Models;
using Demo.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Demo.Web.Controllers;

public class VerifyController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly EmailService _emailservice;
    private readonly string _apiBaseUrl = "http://localhost:5114/api/ForgotPassword/";
    public VerifyController(IHttpClientFactory httpClientFactory, EmailService emailService)
    {
        _httpClient = httpClientFactory.CreateClient();
        _emailservice = emailService;
    }
    public IActionResult Index()
    {
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> VerifyOtp([FromForm] OtpViewModel otpViewModel)
    {
        if (!ModelState.IsValid)
        {
            return new JsonResult(new { success = false, message = "Invalid OTP." });
        }

        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(_apiBaseUrl + "verify", otpViewModel);
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
