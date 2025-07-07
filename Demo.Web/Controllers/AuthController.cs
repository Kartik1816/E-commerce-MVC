using System.Text.Json;
using Demo.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Demo.Web.Controllers;

public class AuthController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl = "http://localhost:5114/api/Auth/";

    public AuthController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
    }
    public IActionResult Index()
    {
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> Login([FromForm] AuthViewModel authViewModel)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        HttpResponseMessage response;

        response = await _httpClient.PostAsJsonAsync(_apiBaseUrl + "login", authViewModel);

        if (response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            ResponseModel? responseData = JsonConvert.DeserializeObject<ResponseModel>(responseContent);

            if (responseData != null)
            {
                if (responseData.IsSuccess)
                {
                    dynamic? data = responseData.Data as dynamic;
                    string token = data?.token ?? string.Empty;
                    string refreshToken = data?.refreshToken ?? string.Empty;

                    return new JsonResult(new 
                    { 
                        success = responseData.IsSuccess, 
                        message = responseData.Message, 
                        token = token, 
                        refreshToken = refreshToken 
                    });
                }
                else
                {
                    return new JsonResult(new 
                    { 
                        success = responseData.IsSuccess, 
                        message = responseData.Message 
                    });
                }
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
    [Route("Auth/RefreshToken")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync(_apiBaseUrl + "refreshtoken", request.RefreshToken);

        if (response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            dynamic? responseData = JsonConvert.DeserializeObject<dynamic>(responseContent);
            if (responseData != null)
            {
                string token = responseData.accessToken;
                string refreshToken = responseData.refreshToken;
                return new JsonResult(new
                {
                    token = token,
                    refreshToken = refreshToken
                });
            }
            else
            {
                return new JsonResult(new { success = false, message = "Invalid response from server." });
            }
        }

        return Unauthorized();
    }
    [HttpGet]
    public IActionResult Registration()
    {
        return View();
    }

    [HttpPost]
    [Route("/Auth/Registration")] 
    public async Task<IActionResult> Registration([FromForm] RegistrationViewModel registrationViewModel)
    {
         if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        using (var content = new MultipartFormDataContent())
        {
            // Add other form data
            content.Add(new StringContent(registrationViewModel.FirstName ?? ""), "FirstName");
            content.Add(new StringContent(registrationViewModel.LastName ?? ""), "LastName");
            content.Add(new StringContent(registrationViewModel.PhoneNumber ?? ""), "PhoneNumber");
            content.Add(new StringContent(registrationViewModel.Address ?? ""), "Address");
            content.Add(new StringContent(registrationViewModel.RoleId.ToString()), "RoleId");
            content.Add(new StringContent(registrationViewModel.ConfirmPassword ?? ""), "ConfirmPassword");
            content.Add(new StringContent(registrationViewModel.Email ?? ""), "Email");
            content.Add(new StringContent(registrationViewModel.Password ?? ""), "Password");

            // Add the image file if it exists
            if (registrationViewModel.Image != null)
            {
                var fileStream = registrationViewModel.Image.OpenReadStream();
                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(registrationViewModel.Image.ContentType);
                content.Add(fileContent, "Image", registrationViewModel.Image.FileName);
            }

            // Send the request to the API
            HttpResponseMessage response = await _httpClient.PostAsync(_apiBaseUrl + "register", content);

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();
                ResponseModel? responseData = JsonConvert.DeserializeObject<ResponseModel>(responseContent);
                if( responseData == null)
                {
                    return new JsonResult(new { success = false, message = "Invalid response from server." });
                }
                return new JsonResult(new { success = responseData.IsSuccess, message = responseData.Message });
            }
            else
            {
                return StatusCode((int)response.StatusCode, "Error occurred while processing the request.");
            }
        }
    }
}
