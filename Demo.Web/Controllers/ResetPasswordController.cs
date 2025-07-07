using System.Threading.Tasks;
using Demo.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Demo.Web.Controllers;

public class ResetPasswordController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl = "http://localhost:5114/api/ForgotPassword/";
    public ResetPasswordController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
    }
    public IActionResult Index()
    {
        return View();
    }
    [HttpPost]

    public async Task<IActionResult> ResetPassword([FromForm] ResetPasswordViewModel resetPasswordViewModel)
    {
        if (!ModelState.IsValid)
        {
            return new JsonResult(new { success = false, message = "Invalid password or confirmation." });
        }
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(_apiBaseUrl + "resetpassword", resetPasswordViewModel);
        if (response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            ResponseModel? responseData = JsonConvert.DeserializeObject<ResponseModel>(responseContent);
            if (responseData != null)
            {
                string? message = responseData.Message;
                bool success = responseData.IsSuccess;
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
