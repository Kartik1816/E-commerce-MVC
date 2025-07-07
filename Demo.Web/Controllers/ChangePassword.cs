using System.Threading.Tasks;
using Demo.Web.Middleware;
using Demo.Web.Models;
using Demo.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Web.Controllers;

[JwtMiddleware]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class ChangePassword : Controller
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl = "http://localhost:5114/api/ChangePassword/";
    public ChangePassword(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
    }
    public IActionResult Index()
    {
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> UpdatePassword([FromForm] ChangePasswordViewModel changePasswordViewModel)
    {
        if (!ModelState.IsValid)
        {
            return new JsonResult(new { success = false, message = "Invalid password or confirmation." });
        }
        string? jwtToken = HttpContext.Request.Cookies["token"];
        if (jwtToken == null)
        {
            return RedirectToAction("Index", "Auth");
        }
        int userId = JwtService.GetUserIdFromJwtToken(jwtToken);
        if (userId <= 0)
        {
            return RedirectToAction("Index", "Auth");
        }
        changePasswordViewModel.Id = userId;
        HttpResponseMessage response = await _httpClient.PostAsJsonAsync(_apiBaseUrl + "updatepassword", changePasswordViewModel);
        if (response.IsSuccessStatusCode)
        {
            string responseContent = response.Content.ReadAsStringAsync().Result;
            ResponseModel? responseData = System.Text.Json.JsonSerializer.Deserialize<ResponseModel>(
                responseContent,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );
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
