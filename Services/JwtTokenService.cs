using API.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Services;

public interface IJwtTokenService
{
    string CreateToken(User user);
    ClaimsPrincipal? ValidateToken(string token);
}

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly TokenValidationParameters _validationParameters;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
        byte[] key = Encoding.UTF8.GetBytes(
            configuration["JwtSettings:Secret"] ?? throw new InvalidOperationException(
                "JwtSettings:Secret is missing."));
        _validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = configuration["JwtSettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = configuration["JwtSettings:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    }

    // [GNS301_Require] JWT chứa identity và role để REST API/TCP chat dùng cùng một phiên đăng nhập.
    public string CreateToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id!),
            new(ClaimTypes.Name, user.Username)
        };
        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(
                double.Parse(_configuration["JwtSettings:ExpiryInMinutes"] ?? "1440")),
            Issuer = _configuration["JwtSettings:Issuer"],
            Audience = _configuration["JwtSettings:Audience"],
            SigningCredentials = new SigningCredentials(
                _validationParameters.IssuerSigningKey,
                SecurityAlgorithms.HmacSha256)
        };
        return new JwtSecurityTokenHandler().WriteToken(
            new JwtSecurityTokenHandler().CreateToken(descriptor));
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            return new JwtSecurityTokenHandler().ValidateToken(
                token,
                _validationParameters,
                out _);
        }
        catch (SecurityTokenException)
        {
            return null;
        }
    }

    public TokenValidationParameters CreateValidationParameters() =>
        _validationParameters.Clone();
}
