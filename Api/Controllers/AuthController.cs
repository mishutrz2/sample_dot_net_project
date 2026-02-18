using Api.Data;
using Api.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace Api.Controllers;

/// <summary>
/// Authentication endpoints for AWS Cognito sign-up, sign-in, token management
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAppAuthenticationService _authService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAppAuthenticationService authService, ApplicationDbContext context, ILogger<AuthController> logger)
    {
        _authService = authService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegistrationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegistrationRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.RegisterAsync(request, cancellationToken);

        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Sign in user and get JWT tokens
    /// </summary>
    [HttpPost("signin")]
    [ProducesResponseType(typeof(AuthenticationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SignIn([FromBody] SignInRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.SignInAsync(request, cancellationToken);
        
        if (!result.Success)
            return Unauthorized(result);

        return Ok(result);
    }

    /// <summary>
    /// Confirm user email with confirmation code
    /// </summary>
    [HttpPost("confirm")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmSignUp([FromQuery] string email, [FromQuery] string code, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code))
            return BadRequest("Email and confirmation code are required");

        var result = await _authService.ConfirmSignUpAsync(email, code, cancellationToken);
        
        if (!result)
            return BadRequest("Failed to confirm email. Code may be expired.");

        return Ok(new { message = "Email confirmed successfully" });
    }

    /// <summary>
    /// Resend confirmation code to user's email
    /// </summary>
    [HttpPost("resend-code")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResendConfirmationCode([FromQuery] string email, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest("Email is required");

        var result = await _authService.ResendConfirmationCodeAsync(email, cancellationToken);
        
        if (!result)
            return BadRequest("Failed to resend confirmation code");

        return Ok(new { message = "Confirmation code sent to your email" });
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthenticationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return BadRequest("Refresh token is required");

        var result = await _authService.RefreshTokenAsync(request.RefreshToken, cancellationToken);
        
        if (!result.Success)
            return Unauthorized(result);

        return Ok(result);
    }

    /// <summary>
    /// Change user password (requires authentication)
    /// </summary>
    [Authorize]
    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var email = User.FindFirst("email")?.Value;
        
        if (string.IsNullOrWhiteSpace(email))
            return Unauthorized("User email not found in token");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.ChangePasswordAsync(email, request.OldPassword, request.NewPassword, cancellationToken);
        
        if (!result)
            return BadRequest("Failed to change password. Old password may be incorrect.");

        return Ok(new { message = "Password changed successfully" });
    }

    /// <summary>
    /// Initiate forgot password flow
    /// </summary>
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword([FromQuery] string email, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest("Email is required");

        var result = await _authService.ForgotPasswordAsync(email, cancellationToken);
        
        if (!result)
            return BadRequest("Failed to initiate password reset");

        return Ok(new { message = "Password reset code sent to your email" });
    }

    /// <summary>
    /// Confirm forgot password and set new password
    /// </summary>
    [HttpPost("confirm-password-reset")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConfirmPasswordReset([FromBody] ConfirmPasswordResetRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _authService.ConfirmForgotPasswordAsync(request.Email, request.Code, request.NewPassword, cancellationToken);
        
        if (!result)
            return BadRequest("Failed to reset password. Code may be expired.");

        return Ok(new { message = "Password reset successfully" });
    }

    /// <summary>
    /// Get authenticated user's profile
    /// </summary>
    [Authorize]
    [HttpGet("profile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
    {
        // Debug: Log all claims
        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        _logger.LogInformation("Token claims: {@Claims}", claims);

        // Get the 'sub' (subject/user ID) from the token
        var sub = User.FindFirst("sub")?.Value;
        
        // Also try 'username' claim as fallback
        if (string.IsNullOrWhiteSpace(sub))
        {
            sub = User.FindFirst("username")?.Value;
        }
        
        if (string.IsNullOrWhiteSpace(sub))
            return Unauthorized(new { message = "User ID not found in token", claims });

        // Look up the user in the database by AwsSubject (could be either sub or username)
        var appUser = await _context.AppUsers.FirstOrDefaultAsync(u => u.AwsSubject == sub, cancellationToken);
        
        if (appUser == null)
            return NotFound(new { message = "User not found in database", userId = sub });

        return Ok(new
        {
            email = appUser.Email,
            userId = sub,
            displayName = appUser.DisplayName,
            authenticated = true
        });
    }

    /// <summary>
    /// Logout endpoint that redirects to Cognito's logout endpoint
    /// This signs out the user from both the application and Cognito
    /// </summary>
    [HttpGet("logout")]
    [Authorize]
    public IActionResult Logout(CancellationToken cancellationToken)
    {
        // Return a SignOutResult so the OpenID Connect middleware can redirect
        // the browser to Cognito's logout endpoint and then back to the application.
        var authProperties = new AuthenticationProperties
        {
            RedirectUri = "/"
        };

        return SignOut(authProperties, OpenIdConnectDefaults.AuthenticationScheme);
    }
}

/// <summary>Request for token refresh</summary>
public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = default!;
}

/// <summary>Request for password change (requires authentication)</summary>
public class ChangePasswordRequest
{
    public string OldPassword { get; set; } = default!;
    public string NewPassword { get; set; } = default!;
}

/// <summary>Request to confirm password reset</summary>
public class ConfirmPasswordResetRequest
{
    public string Email { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string NewPassword { get; set; } = default!;
}
