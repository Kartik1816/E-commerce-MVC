using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;

namespace Demo.Web.Middleware;

public class JwtMiddleware : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext authorizationFilterContext)
    {

        var requestedUrl = authorizationFilterContext.HttpContext.Request.Path.Value;

        if (requestedUrl == null || !IsAuthorized(authorizationFilterContext.HttpContext))
        {
            authorizationFilterContext.Result = new RedirectToRouteResult(new RouteValueDictionary
                {
                    { "controller" , "Auth" },
                    { "action" , "Index" }
                });
        }
    }

    public bool IsAuthorized(HttpContext context)
    {
        string? token = context.Request.Cookies["token"];
        if (string.IsNullOrEmpty(token))
        {
            return false;
        }

        try
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            byte[] key = Encoding.ASCII.GetBytes("b14ca5898a4e4133bbce2ea2315a1916");

            TokenValidationParameters validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = "http://localhost:5114",
                ValidateAudience = true,
                ValidAudience = "http://localhost:5114",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            // Validate the token
            tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
            //Role Id 1 can add products
            if (context.Request.Path.Value?.Contains("AddProduct") == true)
            {
                JwtSecurityToken jwtToken = (JwtSecurityToken)validatedToken;
                string? role = jwtToken.Claims.ElementAt(4).Value;
                if (role == "Admin")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

}
