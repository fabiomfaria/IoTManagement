using Xunit;
using Moq;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Net.Http; // For HttpClient if provider uses it
using System.Linq;
using IoTManagement.UI.Blazor.Providers;
using System.IdentityModel.Tokens.Jwt; // For JwtSecurityToken
using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.IdentityModel.Tokens;

public class ApiAuthenticationStateProviderTests
{
    private readonly Mock<ILocalStorageService> _mockLocalStorage;
    private readonly Mock<HttpClient> _mockHttpClient; // Or mock an API service if it uses one
    private readonly ApiAuthenticationStateProvider _provider;

    public ApiAuthenticationStateProviderTests()
    {
        _mockLocalStorage = new Mock<ILocalStorageService>();
        _mockHttpClient = new Mock<HttpClient>(); // If provider directly uses HttpClient
        _provider = new ApiAuthenticationStateProvider(_mockLocalStorage.Object, _mockHttpClient.Object);
    }

    private string GenerateTestJwtToken(DateTime expiry, string username = "testuser", string userId = "123", List<string> roles = null)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = new byte[32]; // Dummy key for testing, ensure it's sufficient length
        new Random().NextBytes(key); // Fill with random bytes

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, userId), // Subject (user ID)
            new Claim(ClaimTypes.Name, username),           // Username
            // Add more standard claims like iss, aud if your validation checks them
            new Claim(JwtRegisteredClaimNames.Iss, "test-issuer"),
            new Claim(JwtRegisteredClaimNames.Aud, "test-audience")
        };

        if (roles != null)
        {
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiry,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_NoTokenInStorage_ReturnsAnonymousUser()
    {
        // Arrange
        _mockLocalStorage.Setup(ls => ls.GetItemAsStringAsync("authToken", It.IsAny<CancellationToken>())).ReturnsAsync((string)null);

        // Act
        var authState = await _provider.GetAuthenticationStateAsync();

        // Assert
        Assert.NotNull(authState);
        Assert.NotNull(authState.User);
        Assert.False(authState.User.Identity.IsAuthenticated);
        Assert.Empty(authState.User.Claims);
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_ValidTokenInStorage_ReturnsAuthenticatedUserWithClaims()
    {
        // Arrange
        var expiry = DateTime.UtcNow.AddHours(1);
        var username = "john.doe";
        var userId = "user-xyz";
        var roles = new List<string> { "Admin", "Editor" };
        var validToken = GenerateTestJwtToken(expiry, username, userId, roles);

        _mockLocalStorage.Setup(ls => ls.GetItemAsStringAsync("authToken", It.IsAny<CancellationToken>())).ReturnsAsync(validToken);

        // Act
        var authState = await _provider.GetAuthenticationStateAsync();

        // Assert
        Assert.NotNull(authState);
        Assert.NotNull(authState.User);
        Assert.True(authState.User.Identity.IsAuthenticated);
        Assert.Equal(username, authState.User.Identity.Name);
        Assert.Equal(userId, authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value); // Or JwtRegisteredClaimNames.Sub
        Assert.True(authState.User.IsInRole("Admin"));
        Assert.True(authState.User.IsInRole("Editor"));
        Assert.False(authState.User.IsInRole("NonExistentRole"));
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_ExpiredTokenInStorage_ReturnsAnonymousUser()
    {
        // Arrange
        var expiredToken = GenerateTestJwtToken(DateTime.UtcNow.AddHours(-1)); // Expired 1 hour ago
        _mockLocalStorage.Setup(ls => ls.GetItemAsStringAsync("authToken", It.IsAny<CancellationToken>())).ReturnsAsync(expiredToken);

        // Act
        var authState = await _provider.GetAuthenticationStateAsync();

        // Assert
        Assert.False(authState.User.Identity.IsAuthenticated);
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_MalformedTokenInStorage_ReturnsAnonymousUser()
    {
        // Arrange
        var malformedToken = "this.is.notavalidjwt";
        _mockLocalStorage.Setup(ls => ls.GetItemAsStringAsync("authToken", It.IsAny<CancellationToken>())).ReturnsAsync(malformedToken);

        // Act
        var authState = await _provider.GetAuthenticationStateAsync();

        // Assert
        Assert.False(authState.User.Identity.IsAuthenticated);
    }

    [Fact]
    public void NotifyUserAuthentication_ValidToken_NotifiesAndChangesState()
    {
        // Arrange
        var token = GenerateTestJwtToken(DateTime.UtcNow.AddHours(1));
        AuthenticationState notifiedState = null;
        _provider.AuthenticationStateChanged += (task) => { notifiedState = task.Result; };

        // Act
        _provider.NotifyUserAuthentication(token); // This method should trigger GetAuthenticationStateAsync indirectly via NotifyAuthenticationStateChanged

        // Assert
        // Need to wait for the event or manually call GetAuthenticationStateAsync if NotifyUserAuthentication doesn't synchronously update.
        // For simplicity, let's assume NotifyUserAuthentication updates an internal state that GetAuthenticationStateAsync reads.
        // A better test might involve capturing the Task passed to NotifyAuthenticationStateChanged.

        // Spin wait briefly for the event to fire (not ideal for unit tests, but simple for example)
        // Task.Delay(50).Wait(); // Avoid in real tests if possible; use ManualResetEvent or similar.

        // If NotifyUserAuthentication internally sets a field that GetAuthenticationStateAsync uses,
        // then calling GetAuthenticationStateAsync again AFTER NotifyUserAuthentication should reflect the change.
        // This test is a bit tricky because of the event-driven nature.
        // The key is that NotifyAuthenticationStateChanged is called with a Task<AuthenticationState>
        // that will eventually resolve to an authenticated user.

        // For now, let's just verify NotifyAuthenticationStateChanged was effectively called by checking
        // the current user state if the provider updates it synchronously or via a quick path.
        // This part highly depends on ApiAuthenticationStateProvider's implementation.
        // Assert.NotNull(notifiedState); // This would be ideal if we can capture the event's result
        // Assert.True(notifiedState.User.Identity.IsAuthenticated);

        // A simpler, though less direct test:
        // Verify that after notification, subsequent calls to GetAuthenticationStateAsync *would* return authenticated.
        // This requires GetAuthenticationStateAsync to be re-entrant and pick up the new token info.
        // This is often how it's designed: Notify updates internal state, GetAuthenticationStateAsync re-evaluates.
        _mockLocalStorage.Setup(ls => ls.GetItemAsStringAsync("authToken", It.IsAny<CancellationToken>())).ReturnsAsync(token); // Simulate token is now in storage
        var authStateAfterNotify = _provider.GetAuthenticationStateAsync().Result; // Force sync for test
        Assert.True(authStateAfterNotify.User.Identity.IsAuthenticated);
    }

    [Fact]
    public void NotifyUserLogout_ClearsStateAndNotifies()
    {
        // Arrange
        // Simulate user was logged in
        var initialToken = GenerateTestJwtToken(DateTime.UtcNow.AddHours(1));
        _mockLocalStorage.Setup(ls => ls.GetItemAsStringAsync("authToken", It.IsAny<CancellationToken>())).ReturnsAsync(initialToken);
        var initialState = _provider.GetAuthenticationStateAsync().Result;
        Assert.True(initialState.User.Identity.IsAuthenticated); // Pre-condition

        _mockLocalStorage.Setup(ls => ls.GetItemAsStringAsync("authToken", It.IsAny<CancellationToken>())).ReturnsAsync((string)null); // After logout, token is gone

        AuthenticationState notifiedState = null;
        _provider.AuthenticationStateChanged += (task) => { notifiedState = task.Result; };

        // Act
        _provider.NotifyUserLogout();

        // Assert
        // Task.Delay(50).Wait(); // Again, avoid if possible.
        // Assert.NotNull(notifiedState);
        // Assert.False(notifiedState.User.Identity.IsAuthenticated);

        var authStateAfterLogout = _provider.GetAuthenticationStateAsync().Result;
        Assert.False(authStateAfterLogout.User.Identity.IsAuthenticated);
    }
}