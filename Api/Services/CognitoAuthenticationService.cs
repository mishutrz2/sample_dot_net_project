using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Runtime;
using Api.Data;
using Api.Models;
using Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;

namespace Api.Services;

/// <summary>
/// AWS Cognito implementation of authentication service
/// Manages user registration, sign-in, and token handling
/// </summary>
public class CognitoAuthenticationService : IAppAuthenticationService
{
    private readonly AmazonCognitoIdentityProviderClient _cognitoClient;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CognitoAuthenticationService> _logger;
    private readonly string _userPoolId;
    private readonly string _clientId;
    private readonly string? _clientSecret;

    public CognitoAuthenticationService(
        IConfiguration configuration,
        ApplicationDbContext context,
        ILogger<CognitoAuthenticationService> logger)
    {
        _context = context;
        _logger = logger;
        
        var region = configuration["AWS:Region"] ?? "us-east-1";
        _userPoolId = configuration["Cognito:UserPoolId"] ?? throw new InvalidOperationException("Missing Cognito:UserPoolId config");
        _clientId = configuration["Cognito:ClientId"] ?? throw new InvalidOperationException("Missing Cognito:ClientId config");
        _clientSecret = configuration["Cognito:ClientSecret"]; // Optional - if app client has secret

        // Create Cognito client with AWS credentials if provided, otherwise use default credentials
        var accessKey = configuration["AWS:AccessKey"];
        var secretKey = configuration["AWS:SecretKey"];

        if (!string.IsNullOrEmpty(accessKey) && !string.IsNullOrEmpty(secretKey))
        {
            var credentials = new BasicAWSCredentials(accessKey, secretKey);
            _cognitoClient = new AmazonCognitoIdentityProviderClient(credentials, RegionEndpoint.GetBySystemName(region));
        }
        else
        {
            // Fall back to default AWS credentials (environment variables, AWS CLI, IAM role, etc.)
            _cognitoClient = new AmazonCognitoIdentityProviderClient(RegionEndpoint.GetBySystemName(region));
        }
    }

    /// <summary>
    /// Register a new user with AWS Cognito
    /// Also creates an AppUser record in our database
    /// </summary>
    public async Task<RegistrationResult> RegisterAsync(RegistrationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var userAttributes = new List<AttributeType>
            {
                new AttributeType { Name = "email", Value = request.Email },
                new AttributeType { Name = "name", Value = request.DisplayName },
                new AttributeType { Name = "gender", Value = request.Gender ?? "other" },
                new AttributeType { Name = "nickname", Value = request.NickName ?? request.DisplayName }
            };

            if (request.DateOfBirth.HasValue)
            {
                userAttributes.Add(new AttributeType
                {
                    Name = "birthdate",
                    Value = request.DateOfBirth.Value.ToString("yyyy-MM-dd")
                });
            }

            var signUpRequest = new SignUpRequest
            {
                ClientId = _clientId,
                Username = request.Email,
                Password = request.Password,
                UserAttributes = userAttributes
            };

            // Add SecretHash if client has a secret configured
            if (!string.IsNullOrEmpty(_clientSecret))
            {
                signUpRequest.SecretHash = CalculateSecretHash(request.Email);
            }

            var response = await _cognitoClient.SignUpAsync(signUpRequest, cancellationToken);

            // Create AppUser in our database after successful Cognito registration
            var appUser = new AppUser
            {
                AwsSubject = response.UserSub, // Cognito user ID
                Email = request.Email,
                DisplayName = request.DisplayName,
                IsActive = false, // Not active until email confirmed
                DateOfBirth = request.DateOfBirth.HasValue 
                    ? DateTime.SpecifyKind(request.DateOfBirth.Value, DateTimeKind.Utc)
                    : null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.AppUsers.Add(appUser);
            await _context.SaveChangesAsync(cancellationToken);

            return new RegistrationResult
            {
                Success = true,
                Message = "User registered successfully. Please check your email for confirmation code.",
                UserId = response.UserSub,
                ConfirmationRequired = "email"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration: {Message}", ex.Message);
            return new RegistrationResult
            {
                Success = false,
                Message = "Registration failed. Please try again later."
            };
        }
    }

    /// <summary>
    /// Sign in user with email and password
    /// Returns JWT tokens if successful
    /// </summary>
    public async Task<AuthenticationResult> SignInAsync(SignInRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var authRequest = new AdminInitiateAuthRequest
            {
                UserPoolId = _userPoolId,
                ClientId = _clientId,
                AuthFlow = AuthFlowType.ADMIN_NO_SRP_AUTH,
                AuthParameters = new Dictionary<string, string>
                {
                    { "USERNAME", request.Email },
                    { "PASSWORD", request.Password }
                }
            };

            // Add SecretHash if client has a secret configured
            if (!string.IsNullOrEmpty(_clientSecret))
            {
                authRequest.AuthParameters["SECRET_HASH"] = CalculateSecretHash(request.Email);
            }

            var response = await _cognitoClient.AdminInitiateAuthAsync(authRequest, cancellationToken);

            if (response.AuthenticationResult == null)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    Message = "Authentication failed"
                };
            }

            // Get the Cognito user sub from the ID token
            var userSubject = ExtractClaimFromToken(response.AuthenticationResult.IdToken, "sub");

            // If we cannot extract a valid subject, treat this as an authentication failure
            if (string.IsNullOrEmpty(userSubject))
            {
                return new AuthenticationResult
                {
                    Success = false,
                    Message = "Authentication failed"
                };
            }
            // Update AppUser's last login
            var appUser = await _context.AppUsers.FirstOrDefaultAsync(u => u.AwsSubject == userSubject, cancellationToken);
            if (appUser != null)
            {
                appUser.UpdatedAt = DateTime.UtcNow;
                _context.AppUsers.Update(appUser);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return new AuthenticationResult
            {
                Success = true,
                Message = "Sign in successful",
                AccessToken = response.AuthenticationResult.AccessToken,
                RefreshToken = response.AuthenticationResult.RefreshToken,
                ExpiresIn = response.AuthenticationResult.ExpiresIn,
                UserId = userSubject,
                Email = request.Email
            };
        }
        catch (UserNotConfirmedException)
        {
            return new AuthenticationResult
            {
                Success = false,
                Message = "User email not confirmed. Please check your email for confirmation code."
            };
        }
        catch (NotAuthorizedException)
        {
            return new AuthenticationResult
            {
                Success = false,
                Message = "Invalid email or password"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during sign in: {Message}", ex.Message);
            return new AuthenticationResult
            {
                Success = false,
                Message = "Sign in failed. Please try again later."
            };
        }
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    public async Task<AuthenticationResult> RefreshTokenAsync(string refreshToken, string username, CancellationToken cancellationToken = default)
    {
        try
        {
            var authRequest = new InitiateAuthRequest
            {
                ClientId = _clientId,
                AuthFlow = AuthFlowType.REFRESH_TOKEN_AUTH,
                AuthParameters = new Dictionary<string, string>
                {
                    { "REFRESH_TOKEN", refreshToken },
                    { "USERNAME", username }
                }
            };

            // Add SecretHash if client has a secret configured
            if (!string.IsNullOrEmpty(_clientSecret))
            {
                authRequest.AuthParameters["SECRET_HASH"] = CalculateSecretHash(username);
            }

            var response = await _cognitoClient.InitiateAuthAsync(authRequest, cancellationToken);

            if (response.AuthenticationResult == null)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    Message = "Token refresh failed"
                };
            }

            return new AuthenticationResult
            {
                Success = true,
                Message = "Token refreshed successfully",
                AccessToken = response.AuthenticationResult.AccessToken,
                ExpiresIn = response.AuthenticationResult.ExpiresIn,
                RefreshToken = response.AuthenticationResult.RefreshToken ?? refreshToken
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token: {Message}", ex.Message);
            return new AuthenticationResult
            {
                Success = false,
                Message = "Token refresh failed"
            };
        }
    }

    /// <summary>
    /// Confirm user email/phone with confirmation code
    /// </summary>
    public async Task<bool> ConfirmSignUpAsync(string username, string confirmationCode, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new ConfirmSignUpRequest
            {
                ClientId = _clientId,
                Username = username,
                ConfirmationCode = confirmationCode
            };

            // Add SecretHash if client has a secret configured
            if (!string.IsNullOrEmpty(_clientSecret))
            {
                request.SecretHash = CalculateSecretHash(username);
            }

            await _cognitoClient.ConfirmSignUpAsync(request, cancellationToken);

            // Mark user as active in our database
            var appUser = await _context.AppUsers.FirstOrDefaultAsync(u => u.Email == username, cancellationToken);
            if (appUser != null)
            {
                appUser.IsActive = true;
                appUser.UpdatedAt = DateTime.UtcNow;
                _context.AppUsers.Update(appUser);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming sign up: {Message}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Resend confirmation code to user
    /// </summary>
    public async Task<bool> ResendConfirmationCodeAsync(string username, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new ResendConfirmationCodeRequest
            {
                ClientId = _clientId,
                Username = username
            };

            // Add SecretHash if client has a secret configured
            if (!string.IsNullOrEmpty(_clientSecret))
            {
                request.SecretHash = CalculateSecretHash(username);
            }

            await _cognitoClient.ResendConfirmationCodeAsync(request, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending confirmation code: {Message}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Change user password (when user knows current password)
    /// </summary>
    public async Task<bool> ChangePasswordAsync(string username, string oldPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        try
        {
            // First authenticate to get access token
            var authResult = await SignInAsync(new SignInRequest { Email = username, Password = oldPassword }, cancellationToken);
            if (!authResult.Success || authResult.AccessToken == null)
                return false;

            var request = new ChangePasswordRequest
            {
                PreviousPassword = oldPassword,
                ProposedPassword = newPassword,
                AccessToken = authResult.AccessToken
            };

            await _cognitoClient.ChangePasswordAsync(request, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password: {Message}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Initiate forgot password flow
    /// </summary>
    public async Task<bool> ForgotPasswordAsync(string username, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new ForgotPasswordRequest
            {
                ClientId = _clientId,
                Username = username
            };

            // Add SecretHash if client has a secret configured
            if (!string.IsNullOrEmpty(_clientSecret))
            {
                request.SecretHash = CalculateSecretHash(username);
            }

            await _cognitoClient.ForgotPasswordAsync(request, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating forgot password: {Message}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Confirm forgot password and set new password
    /// </summary>
    public async Task<bool> ConfirmForgotPasswordAsync(string username, string confirmationCode, string newPassword, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new ConfirmForgotPasswordRequest
            {
                ClientId = _clientId,
                Username = username,
                ConfirmationCode = confirmationCode,
                Password = newPassword
            };

            // Add SecretHash if client has a secret configured
            if (!string.IsNullOrEmpty(_clientSecret))
            {
                request.SecretHash = CalculateSecretHash(username);
            }

            await _cognitoClient.ConfirmForgotPasswordAsync(request, cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming forgot password: {Message}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Extract claim value from JWT token using proper JWT parsing.
    /// Uses JwtSecurityTokenHandler to correctly handle base64url encoding.
    /// </summary>
    private string? ExtractClaimFromToken(string token, string claimType)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            return jwtToken.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Calculate SECRET_HASH for Cognito client secret authentication
    /// Required when Cognito app client has a client secret configured
    /// </summary>
    private string CalculateSecretHash(string username)
    {
        if (string.IsNullOrEmpty(_clientSecret))
            return string.Empty;

        var message = username + _clientId;
        var keyBytes = Encoding.UTF8.GetBytes(_clientSecret);
        var messageBytes = Encoding.UTF8.GetBytes(message);

        using (var hmac = new HMACSHA256(keyBytes))
        {
            var hashBytes = hmac.ComputeHash(messageBytes);
            return Convert.ToBase64String(hashBytes);
        }
    }
}
