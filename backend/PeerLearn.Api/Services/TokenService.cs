using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PeerLearn.Api.Models;

namespace PeerLearn.Api.Services;

public class JwtSettings
{
    public string Key { get; set; } = "";
    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
    public int ExpiryMinutes { get; set; } = 120;
}

public class TokenService(IOptions<JwtSettings> options)
{
    private readonly JwtSettings _s = options.Value;

    public string Create(User u)
    {
        var claims = new[]
        {
            new Claim("uid", u.Id),
            new Claim("name", u.Name),
            new Claim(JwtRegisteredClaimNames.Email, u.Email),
        };
        var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_s.Key)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(_s.Issuer, _s.Audience, claims, expires: DateTime.UtcNow.AddMinutes(_s.ExpiryMinutes), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
