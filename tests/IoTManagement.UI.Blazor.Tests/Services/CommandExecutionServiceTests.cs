using Xunit;
using Moq;
using RichardSzalay.MockHttp;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using IoTManagement.UI.Blazor.Services;
using IoTManagement.Application.DTOs; // For command DTOs
using IoTManagement.UI.Blazor.Models; // For UI specific models if any (e.g. CommandExecutionUIRequest)
using System;
using System.Collections.Generic;

public class BlazorCommandExecutionServiceTests // Renamed
{
    private readonly MockHttpMessageHandler _mockHttp;
    private readonly HttpClient _httpClient;
    private readonly Mock<ILogger<CommandExecutionService>> _mockLogger;
    private readonly CommandExecutionService _service;

    public BlazorCommandExecutionServiceTests()
    {
        _mockHttp = new MockHttpMessageHandler();
        _httpClient = _mockHttp.ToHttpClient();
        _httpClient.BaseAddress = new Uri("http://localhost/api/"); // Adjust

        _mockLogger = new Mock<ILogger<CommandExecutionService>>();
        _service = new CommandExecutionService(_httpClient, _mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteDeviceCommandAsync_SuccessfulApiCall_ReturnsResponseDto()
    {
        // Arrange
        var deviceId = "dev789";
        var commandName = "get_status";
        // Assuming UI has its own model for parameters, or uses a generic Dictionary
        var uiParams = new Dictionary<string, string> { { "format", "full" } };

        var expectedApiResponse = new DeviceCommandExecutionResponseDto // From Application.DTOs
        {
            IsSuccess = true,
            Result = "STATUS: OK, Battery: 90%",
            FormattedResult = "Device Status: OK\nBattery Level: 90%"
        };
        var jsonResponse = JsonSerializer.Serialize(expectedApiResponse);

        _mockHttp.When(HttpMethod.Post, $"http://localhost/api/devices/{deviceId}/commands/{commandName}/execute")
                 .WithJsonContent(new { parameters = uiParams }) // API might expect parameters nested
                 .Respond("application/json", jsonResponse);

        // Act
        var result = await _service.ExecuteDeviceCommandAsync(deviceId, commandName, uiParams);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedApiResponse.Result, result.Result);
        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task ExecuteDeviceCommandAsync_FailedApiCall_ReturnsResponseDtoWithIsSuccessFalse()
    {
        // Arrange
        var deviceId = "dev789";
        var commandName = "bad_command";
        var uiParams = new Dictionary<string, string>();

        var errorResponse = new DeviceCommandExecutionResponseDto { IsSuccess = false, ErrorMessage = "Command failed on device" };
        // The API might return a ProblemDetails for errors, or the service might construct the DTO.
        // If API returns ProblemDetails:
        // _mockHttp.When(...)
        //          .Respond(System.Net.HttpStatusCode.BadRequest, "application/problem+json", JsonSerializer.Serialize(new { title = "Command failed" }));
        // If API returns the DTO directly for failures:
        _mockHttp.When(HttpMethod.Post, $"http://localhost/api/devices/{deviceId}/commands/{commandName}/execute")
                 .Respond(System.Net.HttpStatusCode.BadRequest, "application/json", JsonSerializer.Serialize(errorResponse));


        // Act
        var result = await _service.ExecuteDeviceCommandAsync(deviceId, commandName, uiParams);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Equal(errorResponse.ErrorMessage, result.ErrorMessage); // Or check based on ProblemDetails
        _mockHttp.VerifyNoOutstandingExpectation();
    }
}