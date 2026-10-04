using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using PeerLearn.Api.Common;
using PeerLearn.Api.Data;
using PeerLearn.Api.Hubs;
using PeerLearn.Api.Services;

var builder = WebApplication.CreateBuilder(args);
var cfg = builder.Configuration;

builder.Services.Configure<MongoSettings>(cfg.GetSection("Mongo"));
builder.Services.Configure<JwtSettings>(cfg.GetSection("Jwt"));
builder.Services.AddSingleton<MongoContext>();
builder.Services.AddSingleton<TokenService>();
builder.Services.AddSingleton<IUserIdProvider, UidProvider>();

var jwt = cfg.GetSection("Jwt").Get<JwtSettings>()!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidIssuer = jwt.Issuer,
        ValidateAudience = true, ValidAudience = jwt.Audience,
        ValidateLifetime = true, ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
        ClockSkew = TimeSpan.FromMinutes(1)
    };
    // SignalR (WebSockets) can't send headers, so it passes the token in the query string.
    o.Events = new JwtBearerEvents
    {
        OnMessageReceived = ctx =>
        {
            var token = ctx.Request.Query["access_token"];
            if (!string.IsNullOrEmpty(token) && ctx.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                ctx.Token = token;
            return Task.CompletedTask;
        }
    };
});
builder.Services.AddAuthorization();
builder.Services.AddSignalR();
builder.Services.AddControllers().AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddCors(o => o.AddPolicy("web", p => p
    .WithOrigins(cfg.GetSection("Cors:Origins").Get<string[]>() ?? Array.Empty<string>())
    .AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

var app = builder.Build();

app.UseCors("web");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");
app.Run();
