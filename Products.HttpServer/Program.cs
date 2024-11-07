using System.Net;
using System.Reflection;
using System.Text;
using MadSoul.AspCommon;
using Products.HttpServer;
using MadSoul.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddSingleton<TokenService>();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
        };
    });

//auth policy 
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"))
    .AddPolicy("RequiresManagerClaim", policy =>
        policy.RequireClaim("Department", "Management"));

builder.Services.AddEndpointsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// var apiConfigs = ApiConfigurationExtensions.ListApiConfigInterfaces();
// foreach (var apiConfig in apiConfigs)
// {
//     builder.Services.AddSingleton(apiConfig.defenation , apiConfig.implementation);
// }

//configure api basic settings for this service
builder.Services.AddSingleton<IApi1Configuration, Api1Configuration>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options => { options.RouteTemplate = "/openapi/{documentName}.json"; });
    app.MapScalarApiReference();

    // app.UseSwagger();
    // app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapPost("/login", (AuthRequest request, TokenService tokenService) =>
{
    // Basic hardcoded username/password check for demo purposes
    if (request.Username == "testuser" && request.Password == "password123")
    {
        var tokens = tokenService.GenerateTokens(request.Username);
        return Results.Ok(tokens);
    }

    return Results.Unauthorized();
});

app.MapPost("/rotate-token", (string refreshToken, TokenService tokenService) =>
{
    var newAccessToken = tokenService.RotateAccessToken(refreshToken);
    return newAccessToken != null
        ? Results.Ok(new { AccessToken = newAccessToken })
        : Results.Unauthorized();
});

app.MapGet("/protected", () => "This is a protected endpoint.")
    .RequireAuthorization();

app.MapGet("/protected2", () => "This is a protected endpoint.")
    .RequireAuthorization("AdminOnly");//policy name

app.UseAuthentication();
app.UseAuthorization();
app.RegisterEndpoints();


app.Run();


public interface IApi1Configuration : IApiConfiguration;

public class Api1Configuration : IApi1Configuration
{
    public string BaseUrl { get; } = "";
}