using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using IoTManagement.API.Controllers;
using IoTManagement.Application.Interfaces;
using IoTManagement.Application.DTOs; // For TokenRequestDto, TokenResponseDto
using IoTManagement.API.Models; // For API specific request/response models if different

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly Mock<ILogger<AuthController>> _mockLogger;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mockAuthService = new Mock<IAuthService>();
        _mockLogger = new Mock<ILogger<AuthController>>();
        _controller = new AuthController(_mockAuthService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        // Assuming API.Models.TokenRequestModel is used for the request
        var loginRequest = new TokenRequestModel { Username = "testuser", Password = "password" };

        // And Application.DTOs.OAuth2TokenResponseDto is returned by the service
        var tokenResponseDto = new OAuth2TokenResponseDto
        {
            AccessToken = "fake-access-token",
            RefreshToken = "fake-refresh-token",
            ExpiresIn = 3600,
            TokenType = "Bearer"
        };

        _mockAuthService.Setup(s => s.LoginAsync(It.Is<OAuth2TokenRequestDto>(r => r.Username == loginRequest.Username)))
            .ReturnsAsync(tokenResponseDto);

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actualTokenResponse = Assert.IsType<OAuth2TokenResponseDto>(okResult.Value);
        Assert.Equal(tokenResponseDto.AccessToken, actualTokenResponse.AccessToken);
        _mockAuthService.Verify(s => s.LoginAsync(It.Is<OAuth2TokenRequestDto>(r => r.Username == loginRequest.Username && r.Password == loginRequest.Password)), Times.Once);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new TokenRequestModel { Username = "wronguser", Password = "wrongpassword" };
        _mockAuthService.Setup(s => s.LoginAsync(It.IsAny<OAuth2TokenRequestDto>()))
            .ReturnsAsync((OAuth2TokenResponseDto)null); // Or throw specific exception handled by middleware

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result.Result); // Or UnauthorizedResult if no object is passed
    }

    [Fact]
    public async Task RefreshToken_ValidRequest_ReturnsOkWithNewToken()
    {
        // Arrange
        var refreshTokenRequest = new RefreshTokenRequestModel { RefreshToken = "valid-refresh-token" };
        var newTokenResponseDto = new OAuth2TokenResponseDto { AccessToken = "new-access-token", ExpiresIn = 3600, TokenType = "Bearer" };

        _mockAuthService.Setup(s => s.RefreshTokenAsync(It.Is<OAuth2RefreshTokenRequestDto>(r => r.RefreshToken == refreshTokenRequest.RefreshToken)))
            .ReturnsAsync(newTokenResponseDto);

        // Act
        var result = await _controller.RefreshToken(refreshTokenRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actualTokenResponse = Assert.IsType<OAuth2TokenResponseDto>(okResult.Value);
        Assert.Equal(newTokenResponseDto.AccessToken, actualTokenResponse.AccessToken);
    }

    // TODO: Add tests for RefreshToken_InvalidRequest_ReturnsBadRequestOrUnauthorized
    // TODO: Add tests for RevokeToken if such an endpoint exists
}

