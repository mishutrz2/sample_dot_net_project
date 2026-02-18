using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Api.Services;

/// <summary>
/// Custom logout handler for Cognito that redirects to Cognito's logout endpoint
/// This ensures the user is logged out from Cognito as well as the application
/// </summary>
public class CognitoLogoutHandler : IAuthenticationSignOutHandler
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<CognitoLogoutHandler> _logger;
    private HttpContext? _context;

    public CognitoLogoutHandler(IConfiguration configuration, ILogger<CognitoLogoutHandler> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Task<AuthenticateResult> AuthenticateAsync()
    {
        // This handler is only responsible for sign-out and does not perform authentication.
        // Indicate that no authentication result is provided by this handler.
        return Task.FromResult(AuthenticateResult.NoResult());
    }

    public Task ChallengeAsync(AuthenticationProperties? properties)
    {
        // No challenge behavior is required for this logout-specific handler.
        return Task.CompletedTask;
    }

    public Task ForbidAsync(AuthenticationProperties? properties)
    {
        // No forbid behavior is required for this logout-specific handler.
        return Task.CompletedTask;
    }

    public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
    {
        _context = context;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Handle the sign-out by redirecting to Cognito's logout endpoint
    /// </summary>
    public async Task SignOutAsync(AuthenticationProperties? properties)
    {
        if (_context == null)
            return;

        var cognitoSettings = _configuration.GetSection("Cognito");
        var domain = cognitoSettings["Domain"];
        var clientId = cognitoSettings["ClientId"];
        var logoutRedirectUri = cognitoSettings["LogoutRedirectUri"];

        if (string.IsNullOrEmpty(domain) || string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(logoutRedirectUri))
        {
            _logger.LogError("Cognito configuration missing: Domain, ClientId, or LogoutRedirectUri");
            return;
        }

        // Build the Cognito logout URL
        var logoutUrl = $"{domain}/logout" +
            $"?client_id={Uri.EscapeDataString(clientId)}" +
            $"&logout_uri={Uri.EscapeDataString(logoutRedirectUri)}";

        _context.Response.Redirect(logoutUrl);
    }
}
