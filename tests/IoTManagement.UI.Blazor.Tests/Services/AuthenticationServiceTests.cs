using Xunit;
using Moq;
using RichardSzalay.MockHttp; // For HttpClient
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Components.Authorization; // For AuthenticationStateProvider if needed
using Blazored.LocalStorage; // If used by the service
using IoTManagement.UI.Blazor.Services;
// Use DTOs from Application layer if Blazor services map to/from them for API calls
using IoTManagement.Application.DTOs;
// Or API.Models if Blazor services directly use models matching API schema
using IoTManagement.API.Models; // e.g. TokenRequestModel

public class BlazorAuthenticationServiceTests // Renamed to avoid conflict if you have another AuthenticationService
{
    private readonly MockHttpMessageHandler _mockHttp;
    private readonly HttpClient _httpClient;
    private readonly Mock<ILocalStorageService> _mockLocalStorage;
    private readonly Mock<AuthenticationStateProvider> _mockAuthStateProvider; // e.g., ApiAuthenticationStateProvider
    private readonly Mock<ILogger<AuthenticationService>> _mockLogger;
    private readonly AuthenticationService _service;

    public BlazorAuthenticationServiceTests()
    {
        _mockHttp = new MockHttpMessageHandler();
        _httpClient = _mockHttp.ToHttpClient();
        _httpClient.BaseAddress = new Uri("http://localhost/api/"); // Match your API base address

        _mockLocalStorage = new Mock<ILocalStorageService>();
        _mockAuthStateProvider = new Mock<AuthenticationStateProvider>(); // Or your specific provider type
        _mockLogger = new Mock<ILogger<AuthenticationService>>();

        _service = new AuthenticationService(_httpClient, _mockLocalStorage.Object, _mockAuthStateProvider.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task LoginAsync_SuccessfulApiCall_StoresTokenAndNotifies()
    {
        // Arrange
        var loginModel = new LoginRequest { Username = "test", Password = "pwd" }; // Assuming UI.Blazor.Models.LoginRequest
        var apiTokenResponse = new OAuth2TokenResponseDto // From Application.DTOs
        {
            AccessToken = "fake-jwt-token",
            RefreshToken = "fake-refresh-token",
            ExpiresIn = 3600
        };
        var jsonResponse = JsonSerializer.Serialize(apiTokenResponse);

        _mockHttp.When(HttpMethod.Post, "http://localhost/api/auth/login") // Adjust endpoint
                 .WithJsonContent(loginModel) // Assumes WithJsonContent extension or manual content check
                 .Respond("application/json", jsonResponse);

        // Act
        var result = await _service.LoginAsync(loginModel);

        // Assert
        Assert.True(result); // Assuming LoginAsync returns bool success
        _mockLocalStorage.Verify(ls => ls.SetItemAsStringAsync("authToken", apiTokenResponse.AccessToken, It.IsAny<CancellationToken>()), Times.Once);
        _mockLocalStorage.Verify(ls => ls.SetItemAsStringAsync("refreshToken", apiTokenResponse.RefreshToken, It.IsAny<CancellationToken>()), Times.Once);
        // How to verify NotifyUserAuthentication? If ApiAuthenticationStateProvider is concrete and has a public method called by LoginAsync, mock that.
        // Or, if it raises an event, subscribe and check.
        // If _mockAuthStateProvider is of type ApiAuthenticationStateProvider:
        // ((Mock<ApiAuthenticationStateProvider>)_mockAuthStateProvider).Verify(p => p.NotifyUserAuthentication(apiTokenResponse.AccessToken), Times.Once);
        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task LoginAsync_FailedApiCall_DoesNotStoreTokenAndReturnsFalse()
    {
        // Arrange
        var loginModel = new LoginRequest { Username = "test", Password = "pwd" };
        _mockHttp.When(HttpMethod.Post, "http://localhost/api/auth/login")
                 .Respond(System.Net.HttpStatusCode.Unauthorized);

        // Act
        var result = await _service.LoginAsync(loginModel);

        // Assert
        Assert.False(result);
        _mockLocalStorage.Verify(ls => ls.SetItemAsStringAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LogoutAsync_ClearsStorageAndNotifies()
    {
        // Act
        await _service.LogoutAsync();

        // Assert
        _mockLocalStorage.Verify(ls => ls.RemoveItemAsync("authToken", It.IsAny<CancellationToken>()), Times.Once);
        _mockLocalStorage.Verify(ls => ls.RemoveItemAsync("refreshToken", It.IsAny<CancellationToken>()), Times.Once);
        // ((Mock<ApiAuthenticationStateProvider>)_mockAuthStateProvider).Verify(p => p.NotifyUserLogout(), Times.Once);
    }

    // TODO: Add tests for RefreshTokenAsync if it exists in this service
}

// Helper for WithJsonContent if not using a library that provides it
public static class MockHttpExtensions
{
    public static MockedRequest WithJsonContent<T>(this MockedRequest request, T content)
    {
        return request.WithContent(JsonSerializer.Serialize(content))
                      .WithHeaders("Content-Type", "application/json");
    }
}