using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Products.HttpServer;

public record AuthRequest(string Username, string Password);
public record AuthResponse(string AccessToken, string RefreshToken);

public class JwtSettings
{
    public string SecretKey { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int AccessTokenExpirationMinutes { get; set; }
    public int RefreshTokenExpirationDays { get; set; }
}

public class TokenService
{
    private readonly JwtSettings _jwtSettings;
    private static readonly ConcurrentDictionary<string, string> RefreshTokens = new();

    public TokenService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public AuthResponse GenerateTokens(string username)
    {
        var accessToken = GenerateJwtToken(username);
        var refreshToken = Guid.NewGuid().ToString();

        RefreshTokens[refreshToken] = username; // Store refresh token

        return new AuthResponse(accessToken, refreshToken);
    }

    //this is wrong ! (lonely refresh token is not enugh) 
    public string RotateAccessToken(string refreshToken)
    {
        if (RefreshTokens.TryGetValue(refreshToken, out var username))
        {
            return GenerateJwtToken(username); // Generate new access token
        }
        return null; // Invalid refresh token
    }

    private string GenerateJwtToken(string username)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _jwtSettings.Issuer,
            _jwtSettings.Audience,
            new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            },
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}