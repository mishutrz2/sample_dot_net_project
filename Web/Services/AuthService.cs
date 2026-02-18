using System.Net.Http.Headers;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using Web.Models;

namespace Web.Services;

public interface IAuthService
{
    Task<AuthenticationResult> LoginAsync(LoginRequest request);
    Task<RegistrationResult> RegisterAsync(RegisterRequest request);
    Task<bool> ConfirmEmailAsync(string email, string code);
    Task<bool> ResendConfirmationCodeAsync(string email);
    Task<UserProfile?> GetProfileAsync();
    Task<AuthenticationResult> RefreshTokenAsync();
    Task LogoutAsync();
    Task<bool> IsAuthenticatedAsync();
    Task<string?> GetTokenAsync();
}

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;

    private const string AccessTokenKey = "accessToken";
    private const string RefreshTokenKey = "refreshToken";
    private const string UsernameKey = "username";

    public AuthService(HttpClient httpClient, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
    }

    public async Task<AuthenticationResult> LoginAsync(LoginRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/signin", request);
        var result = await response.Content.ReadFromJsonAsync<AuthenticationResult>()
            ?? new AuthenticationResult { Success = false, Message = "Failed to parse response" };

        if (result.Success && result.AccessToken is not null)
        {
            await _localStorage.SetItemAsStringAsync(AccessTokenKey, result.AccessToken);
            await _localStorage.SetItemAsStringAsync(UsernameKey, request.Email);

            if (result.RefreshToken is not null)
                await _localStorage.SetItemAsStringAsync(RefreshTokenKey, result.RefreshToken);
        }

        return result;
    }

    public async Task<RegistrationResult> RegisterAsync(RegisterRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);
        var result = await response.Content.ReadFromJsonAsync<RegistrationResult>()
            ?? new RegistrationResult { Success = false, Message = "Failed to parse response" };

        // Surface the API error message (e.g. duplicate display name)
        if (!response.IsSuccessStatusCode && string.IsNullOrEmpty(result.Message))
            result.Message = "Registration failed. Please try again.";

        return result;
    }

    public async Task<bool> ConfirmEmailAsync(string email, string code)
    {
        var response = await _httpClient.PostAsync(
            $"api/auth/confirm?email={Uri.EscapeDataString(email)}&code={Uri.EscapeDataString(code)}", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ResendConfirmationCodeAsync(string email)
    {
        var response = await _httpClient.PostAsync(
            $"api/auth/resend-code?email={Uri.EscapeDataString(email)}", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<UserProfile?> GetProfileAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetAsync("api/auth/profile");

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<UserProfile>();
    }

    public async Task<AuthenticationResult> RefreshTokenAsync()
    {
        var refreshToken = await _localStorage.GetItemAsStringAsync(RefreshTokenKey);
        var username = await _localStorage.GetItemAsStringAsync(UsernameKey);

        if (string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(username))
            return new AuthenticationResult { Success = false, Message = "No refresh token available" };

        var request = new RefreshTokenRequest
        {
            RefreshToken = refreshToken,
            Username = username
        };

        var response = await _httpClient.PostAsJsonAsync("api/auth/refresh-token", request);
        var result = await response.Content.ReadFromJsonAsync<AuthenticationResult>()
            ?? new AuthenticationResult { Success = false, Message = "Failed to parse response" };

        if (result.Success && result.AccessToken is not null)
        {
            await _localStorage.SetItemAsStringAsync(AccessTokenKey, result.AccessToken);

            if (result.RefreshToken is not null)
                await _localStorage.SetItemAsStringAsync(RefreshTokenKey, result.RefreshToken);
        }

        return result;
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync(AccessTokenKey);
        await _localStorage.RemoveItemAsync(RefreshTokenKey);
        await _localStorage.RemoveItemAsync(UsernameKey);
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await _localStorage.GetItemAsStringAsync(AccessTokenKey);
        return !string.IsNullOrEmpty(token);
    }

    public async Task<string?> GetTokenAsync()
    {
        return await _localStorage.GetItemAsStringAsync(AccessTokenKey);
    }

    private async Task SetAuthHeaderAsync()
    {
        var token = await _localStorage.GetItemAsStringAsync(AccessTokenKey);
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
