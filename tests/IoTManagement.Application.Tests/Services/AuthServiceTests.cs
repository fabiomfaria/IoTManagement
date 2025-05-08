using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using IoTManagement.Application.Services;
using IoTManagement.Application.DTOs;
using IoTManagement.Domain.Interfaces; // For ITokenService, IUserStore
using IoTManagement.Domain.Entities;   // For User
using IoTManagement.Domain.Exceptions; // For UnauthorizedException

public class AuthServiceTests
{
    private readonly Mock<IUserStore> _mockUserStore;
    private readonly Mock<ITokenService> _mockTokenService;
    private readonly Mock<ILogger<AuthService>> _mockLogger;
    private readonly Application.Services.AuthService _service; // Fully qualify to avoid clash

    public AuthServiceTests()
    {
        _mockUserStore = new Mock<IUserStore>();
        _mockTokenService = new Mock<ITokenService>();
        _mockLogger = new Mock<ILogger<Application.Services.AuthService>>();
        _service = new Application.Services.AuthService(_mockUserStore.Object, _mockTokenService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsTokenResponse()
    {
        // Arrange
        var loginDto = new OAuth2TokenRequestDto { Username = "test", Password = "password" };
        var user = new User { Username = "test", HashedPassword = "hashed_password" /* Assume password check happens here or in UserStore */ };
        var tokenResponse = new OAuth2TokenResponseDto { AccessToken = "xyz", RefreshToken = "abc", ExpiresIn = 3600 };

        _mockUserStore.Setup(s => s.ValidateCredentialsAsync(loginDto.Username, loginDto.Password)).ReturnsAsync(user);
        _mockTokenService.Setup(s => s.GenerateTokensAsync(user)).ReturnsAsync(tokenResponse);

        // Act
        var result = await _service.LoginAsync(loginDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(tokenResponse.AccessToken, result.AccessToken);
        _mockUserStore.Verify(s => s.ValidateCredentialsAsync(loginDto.Username, loginDto.Password), Times.Once);
        _mockTokenService.Verify(s => s.GenerateTokensAsync(user), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_InvalidCredentials_ThrowsUnauthorizedExceptionOrReturnsNull()
    {
        // Arrange
        var loginDto = new OAuth2TokenRequestDto { Username = "test", Password = "wrongpassword" };
        _mockUserStore.Setup(s => s.ValidateCredentialsAsync(loginDto.Username, loginDto.Password))
            .ReturnsAsync((User)null); // Simulate invalid credentials

        // Act & Assert
        // Option 1: Service throws an exception
        await Assert.ThrowsAsync<UnauthorizedException>(() => _service.LoginAsync(loginDto));

        // Option 2: Service returns null (if controller handles null to Unauthorized)
        // var result = await _service.LoginAsync(loginDto);
        // Assert.Null(result);

        _mockTokenService.Verify(s => s.GenerateTokensAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RefreshTokenAsync_ValidToken_ReturnsNewTokenResponse()
    {
        // Arrange
        var refreshTokenDto = new OAuth2RefreshTokenRequestDto { RefreshToken = "valid-refresh" };
        var user = new User { Username = "refreshedUser" };
        var newTokenResponse = new OAuth2TokenResponseDto { AccessToken = "new-access-token", ExpiresIn = 3600 };

        _mockTokenService.Setup(s => s.ValidateRefreshTokenAsync(refreshTokenDto.RefreshToken)).ReturnsAsync(user);
        _mockTokenService.Setup(s => s.GenerateTokensAsync(user)).ReturnsAsync(newTokenResponse); // Assuming new refresh token also generated

        // Act
        var result = await _service.RefreshTokenAsync(refreshTokenDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(newTokenResponse.AccessToken, result.AccessToken);
        _mockTokenService.Verify(s => s.ValidateRefreshTokenAsync(refreshTokenDto.RefreshToken), Times.Once);
        _mockTokenService.Verify(s => s.GenerateTokensAsync(user), Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_InvalidToken_ThrowsUnauthorizedExceptionOrReturnsNull()
    {
        // Arrange
        var refreshTokenDto = new OAuth2RefreshTokenRequestDto { RefreshToken = "invalid-refresh" };
        _mockTokenService.Setup(s => s.ValidateRefreshTokenAsync(refreshTokenDto.RefreshToken))
            .ReturnsAsync((User)null); // Simulate invalid refresh token

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => _service.RefreshTokenAsync(refreshTokenDto));
        // Or:
        // var result = await _service.RefreshTokenAsync(refreshTokenDto);
        // Assert.Null(result);

        _mockTokenService.Verify(s => s.GenerateTokensAsync(It.IsAny<User>()), Times.Never);
    }

    // TODO: Add tests for RevokeTokenAsync if it exists
}
