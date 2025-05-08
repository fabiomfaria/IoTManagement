using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using IoTManagement.Infrastructure.Services; // Where TokenService is
using IoTManagement.Domain.Entities;       // For User
// Assuming you have a JwtSettings class for configuration
public class JwtSettings
{
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public string Key { get; set; }
    public int AccessTokenExpirationMinutes { get; set; }
    public int RefreshTokenExpirationDays { get; set; }
}

public class TokenServiceTests
{
    private readonly Mock<ILogger<TokenService>> _mockLogger;
    private readonly IOptions<JwtSettings> _jwtSettings;
    private readonly TokenService _service;

    public TokenServiceTests()
    {
        _mockLogger = new Mock<ILogger<TokenService>>();
        _jwtSettings = Options.Create(new JwtSettings
        {
            Issuer = "testissuer.com",
            Audience = "testaudience.com",
            Key = "a_super_secret_key_that_is_long_enough_for_hs256", // Must be long enough
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        });
        _service = new TokenService(_jwtSettings, _mockLogger.Object);
    }

    [Fact]
    public async Task GenerateTokensAsync_ValidUser_ReturnsTokenResponseWithTokens()
    {
        // Arrange
        var user = new User { Id = "user123", Username = "testuser", Email = "test@example.com" /*, Roles = new List<string>{"User"}*/ };

        // Act
        var tokenResponse = await _service.GenerateTokensAsync(user);

        // Assert
        Assert.NotNull(tokenResponse);
        Assert.False(string.IsNullOrEmpty(tokenResponse.AccessToken));
        Assert.False(string.IsNullOrEmpty(tokenResponse.RefreshToken));
        Assert.Equal("Bearer", tokenResponse.TokenType);
        Assert.True(tokenResponse.ExpiresIn > 0);

        // Validate Access Token
        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _jwtSettings.Value.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwtSettings.Value.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Value.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        SecurityToken validatedToken;
        var principal = tokenHandler.ValidateToken(tokenResponse.AccessToken, validationParameters, out validatedToken);
        Assert.NotNull(principal);
        Assert.Equal(user.Id, principal.FindFirstValue(ClaimTypes.NameIdentifier));
        Assert.Equal(user.Username, principal.FindFirstValue(ClaimTypes.Name));
        Assert.Equal(user.Email, principal.FindFirstValue(ClaimTypes.Email));
        // Assert.True(principal.IsInRole("User")); // if roles are added
    }

    [Fact]
    public async Task ValidateRefreshTokenAsync_ValidRefreshToken_ReturnsUser()
    {
        // Arrange
        var user = new User { Id = "user456", Username = "refreshuser" };
        // First, generate a refresh token (usually this token is stored and retrieved)
        // For test simplicity, we generate one. In a real scenario, you'd mock a stored token.
        var initialTokens = await _service.GenerateTokensAsync(user);
        var refreshTokenToValidate = initialTokens.RefreshToken;

        // Act
        var validatedUser = await _service.ValidateRefreshTokenAsync(refreshTokenToValidate);

        // Assert
        Assert.NotNull(validatedUser);
        Assert.Equal(user.Id, validatedUser.Id);
        Assert.Equal(user.Username, validatedUser.Username);
    }

    [Fact]
    public async Task ValidateRefreshTokenAsync_InvalidRefreshToken_ReturnsNull()
    {
        // Arrange
        var invalidToken = "this.is.aninvalidtoken";

        // Act
        var validatedUser = await _service.ValidateRefreshTokenAsync(invalidToken);

        // Assert
        Assert.Null(validatedUser);
    }

    [Fact]
    public async Task ValidateRefreshTokenAsync_ExpiredRefreshToken_ReturnsNull()
    {
        // Arrange
        var user = new User { Id = "user789", Username = "expiredUser" };
        // Create a settings object with very short refresh token life for testing expiry
        var expiredSettings = Options.Create(new JwtSettings
        {
            Issuer = _jwtSettings.Value.Issuer,
            Audience = _jwtSettings.Value.Audience,
            Key = _jwtSettings.Value.Key,
            AccessTokenExpirationMinutes = 1,
            RefreshTokenExpirationDays = -1 // Expired yesterday
        });
        var serviceWithExpiredSettings = new TokenService(expiredSettings, _mockLogger.Object);
        var tokens = await serviceWithExpiredSettings.GenerateTokensAsync(user);

        // Act
        var validatedUser = await serviceWithExpiredSettings.ValidateRefreshTokenAsync(tokens.RefreshToken);

        // Assert
        Assert.Null(validatedUser);
    }

    // TODO: Add tests for revoking tokens if that logic is in TokenService
}
