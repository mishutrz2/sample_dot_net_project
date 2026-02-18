using Microsoft.AspNetCore.Identity;

namespace Api.Services.Interfaces;

/// <summary>
/// Authentication service for AWS Cognito
/// Handles user registration, sign-in, and token management
/// </summary>
public interface IAppAuthenticationService
{
    /// <summary>
    /// Register a new user with Cognito
    /// </summary>
    Task<RegistrationResult> RegisterAsync(RegistrationRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sign in user and get tokens
    /// </summary>
    Task<AuthenticationResult> SignInAsync(SignInRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    Task<AuthenticationResult> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirm user email/phone
    /// </summary>
    Task<bool> ConfirmSignUpAsync(string username, string confirmationCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resend confirmation code
    /// </summary>
    Task<bool> ResendConfirmationCodeAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Change user password
    /// </summary>
    Task<bool> ChangePasswordAsync(string username, string oldPassword, string newPassword, CancellationToken cancellationToken = default);

    /// <summary>
    /// Initiate forgot password flow
    /// </summary>
    Task<bool> ForgotPasswordAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirm forgot password and set new password
    /// </summary>
    Task<bool> ConfirmForgotPasswordAsync(string username, string confirmationCode, string newPassword, CancellationToken cancellationToken = default);
}

/// <summary>
/// Request model for user registration
/// </summary>
public class RegistrationRequest
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string DisplayName { get; set; } = default!;
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; } // male, female, or other
    public string? NickName { get; set; }
}

/// <summary>
/// Response model for registration
/// </summary>
public class RegistrationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = default!;
    public string? UserId { get; set; }
    public string? ConfirmationRequired { get; set; }
}

/// <summary>
/// Request model for sign in
/// </summary>
public class SignInRequest
{
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
}

/// <summary>
/// Response model for authentication (tokens)
/// </summary>
public class AuthenticationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = default!;
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public int ExpiresIn { get; set; } // seconds
    public string? UserId { get; set; }
    public string? Email { get; set; }
}
