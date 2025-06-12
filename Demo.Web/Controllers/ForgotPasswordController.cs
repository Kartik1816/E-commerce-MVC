using System.Threading.Tasks;
using Demo.Web.Models;
using Demo.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Demo.Web.Controllers;

public class ForgotPasswordController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly EmailService _emailservice;
    private readonly string _apiBaseUrl = "http://localhost:5114/api/ForgotPassword/";
    public ForgotPasswordController(IHttpClientFactory httpClientFactory, EmailService emailService)
    {
        _httpClient = httpClientFactory.CreateClient();
        _emailservice = emailService;
    }
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ForgotPassword([FromForm] ForgotPasswordViewModel forgotPasswordViewModel)
    {
        if (!ModelState.IsValid)
        {
            return new JsonResult(new { success = false, message = "Invalid email address." });
        }
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(_apiBaseUrl + "forgotpassword", forgotPasswordViewModel.Email);
        if (response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            dynamic? responseData = JsonConvert.DeserializeObject<dynamic>(responseContent);
            if (responseData != null)
            {
                string message = responseData.message;
                bool success = responseData.success;
                int otp = responseData.otp;
                if (success)
                {
                    await _emailservice.SendEmailAsync(forgotPasswordViewModel.Email, otp);
                }
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

    public IActionResult Verify()
    {
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> Verify([FromForm] OtpViewModel otpViewModel)
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
    public IActionResult ResetPassword()
    {
        return View();
    }
}
