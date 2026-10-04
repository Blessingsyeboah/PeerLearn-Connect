using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using PeerLearn.Api.Common;
using PeerLearn.Api.Data;
using PeerLearn.Api.Dtos;
using PeerLearn.Api.Models;
using PeerLearn.Api.Services;

namespace PeerLearn.Api.Controllers;

[ApiController, Route("api/auth")]
public class AuthController(MongoContext db, TokenService tokens) : ControllerBase
{
    static UserDto ToDto(User u) => new(u.Id, u.Name, u.Email, u.IsTutor);

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest r)
    {
        var email = r.Email?.Trim().ToLowerInvariant() ?? "";
        if (string.IsNullOrWhiteSpace(r.Name) || !email.Contains('@')) return BadRequest(new { message = "Enter your name and a valid email." });
        if ((r.Password?.Length ?? 0) < 8) return BadRequest(new { message = "Password must be at least 8 characters." });
        if (await db.Users.Find(u => u.Email == email).AnyAsync()) return Conflict(new { message = "An account with this email already exists." });

        var user = new User { Name = r.Name.Trim(), Email = email, PasswordHash = BCrypt.Net.BCrypt.HashPassword(r.Password) };
        await db.Users.InsertOneAsync(user);
        return Ok(new AuthResponse(tokens.Create(user), ToDto(user)));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest r)
    {
        var email = r.Email?.Trim().ToLowerInvariant() ?? "";
        var user = await db.Users.Find(u => u.Email == email).FirstOrDefaultAsync();
        if (user is null || !BCrypt.Net.BCrypt.Verify(r.Password ?? "", user.PasswordHash))
            return Unauthorized(new { message = "Email or password is incorrect." });
        return Ok(new AuthResponse(tokens.Create(user), ToDto(user)));
    }

    [Authorize, HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var me = User.Uid();
        var user = await db.Users.Find(u => u.Id == me).FirstOrDefaultAsync();
        return user is null ? Unauthorized() : Ok(ToDto(user));
    }
}
