using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Demo.Web.Services;

public class JwtService
{
    public static int GetUserIdFromJwtToken(string token)
    {
        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        JwtSecurityToken? jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

        if (jwtToken == null)
        {
            throw new ArgumentException("Invalid JWT token");
        }

        Claim? userIdClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Sub);
        if (userIdClaim == null)
        {
            throw new ArgumentException("JWT token does not contain a user ID");
        }

        if (!int.TryParse(userIdClaim.Value, out int userId))
        {
            throw new ArgumentException("Invalid user ID in JWT token");
        }

        return userId;

    }
    public static string GetRoleFromJwtToken(string token)
    {
        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        JwtSecurityToken? jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

        if (jwtToken == null)
        {
            throw new ArgumentException("Invalid JWT token");
        }

        Claim? roleClaim = jwtToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Role);
        if (roleClaim == null)
        {
            throw new ArgumentException("JWT token does not contain a role");
        }

        return roleClaim.Value;
    }
}
