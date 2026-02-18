using Api.Data;
using Api.Models;
using Api.Services;
using Api.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultPostgresConnectionString")));

// Application Services
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IEventParticipantService, EventParticipantService>();
builder.Services.AddScoped<IAppAuthenticationService, CognitoAuthenticationService>();

// AWS Cognito Configuration
var cognitoSettings = builder.Configuration.GetSection("Cognito");
var userPoolId = cognitoSettings["UserPoolId"];
var authority = cognitoSettings["Authority"];
var audience = cognitoSettings["ClientId"];
var region = builder.Configuration["AWS:Region"] ?? "us-east-1";

// JWT Bearer Authentication for APIs
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = authority;
    options.Audience = audience;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidAudience = audience,
        ValidateIssuerSigningKey = true,
        ValidateIssuer = builder.Environment.IsProduction(),
        ValidateAudience = builder.Environment.IsProduction(),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(5)
    };
})
// Add OpenID Connect for web-based OAuth2 flows (browser login)
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    options.Authority = authority;
    options.ClientId = cognitoSettings["ClientId"];
    options.ClientSecret = cognitoSettings["ClientSecret"];
    options.ResponseType = "code";
    options.SaveTokens = true;
    
    // Add scopes
    var scopes = cognitoSettings.GetSection("Scopes").Get<List<string>>();
    if (scopes != null)
    {
        foreach (var scope in scopes)
        {
            options.Scope.Add(scope);
        }
    }
    
    // Redirect URIs
    options.CallbackPath = new PathString("/signin-cognito");
    var logoutRedirectUri = cognitoSettings["LogoutRedirectUri"];
    if (!string.IsNullOrEmpty(logoutRedirectUri))
    {
        options.SignedOutRedirectUri = logoutRedirectUri;
    }
    
    // Cookie configuration
    options.NonceCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    
    // Token endpoint authentication
    options.GetClaimsFromUserInfoEndpoint = true;
});

builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();