using Xunit;
using Moq;
using RichardSzalay.MockHttp;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using IoTManagement.UI.Blazor.Services;
// Use Application DTOs if Blazor service maps to/from them for API calls
using IoTManagement.Application.DTOs;
// Or API.Models if Blazor service directly uses models matching API schema
using ApiDeviceModel = IoTManagement.API.Models.DeviceModel; // From API project
using System;
using System.Collections.Generic;
using System.Linq;

public class BlazorDeviceServiceTests // Renamed
{
    private readonly MockHttpMessageHandler _mockHttp;
    private readonly HttpClient _httpClient;
    private readonly Mock<ILogger<DeviceService>> _mockLogger;
    private readonly DeviceService _service;

    public BlazorDeviceServiceTests()
    {
        _mockHttp = new MockHttpMessageHandler();
        _httpClient = _mockHttp.ToHttpClient();
        _httpClient.BaseAddress = new Uri("http://localhost/api/"); // Adjust

        _mockLogger = new Mock<ILogger<DeviceService>>();
        _service = new DeviceService(_httpClient, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllDevicesAsync_SuccessfulApiCall_ReturnsListOfDeviceDtos()
    {
        // Arrange
        // The API /device endpoint returns List<string> (identifiers)
        // Then for each identifier, the UI service might call /device/{id}
        // Or, your API has a different endpoint that returns List<DeviceSummaryDto> or similar.
        // Let's assume for this test the UI service fetches identifiers then full details one by one,
        // or there's an API endpoint that returns full Application.DTOs.DeviceDto models.
        // For simplicity, let's assume API returns list of identifiers, and service then fetches each.

        var deviceIds = new List<string> { "id1", "id2" };
        var device1Details = new ApiDeviceModel { Identifier = "id1", Description = "Device One" };
        var device2Details = new ApiDeviceModel { Identifier = "id2", Description = "Device Two" };

        // Mock for GET /device (list of IDs)
        _mockHttp.When(HttpMethod.Get, "http://localhost/api/device")
                 .Respond("application/json", JsonSerializer.Serialize(deviceIds));

        // Mocks for GET /device/{id} for each ID
        _mockHttp.When(HttpMethod.Get, "http://localhost/api/device/id1")
                 .Respond("application/json", JsonSerializer.Serialize(device1Details));
        _mockHttp.When(HttpMethod.Get, "http://localhost/api/device/id2")
                 .Respond("application/json", JsonSerializer.Serialize(device2Details));

        // Act
        // The Blazor DeviceService.GetAllDevicesAsync might return List<IoTManagement.Application.DTOs.DeviceDto>
        // or List<IoTManagement.API.Models.DeviceModel> depending on its design.
        // Let's assume it returns List<ApiDeviceModel> for directness.
        var result = await _service.GetAllDevicesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, d => d.Identifier == "id1");
        Assert.Contains(result, d => d.Description == "Device Two");
        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetDeviceByIdAsync_SuccessfulApiCall_ReturnsDeviceDto()
    {
        // Arrange
        var deviceId = "dev-xyz";
        var apiResponse = new ApiDeviceModel // From API.Models
        {
            Identifier = deviceId,
            Description = "Specific Device",
            Manufacturer = "Test MFG",
            Url = "telnet://1.2.3.4:23"
        };
        var jsonResponse = JsonSerializer.Serialize(apiResponse);
        _mockHttp.When(HttpMethod.Get, $"http://localhost/api/device/{deviceId}")
                 .Respond("application/json", jsonResponse);

        // Act
        // Assume Blazor DeviceService.GetDeviceByIdAsync returns ApiDeviceModel
        var result = await _service.GetDeviceByIdAsync(deviceId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(deviceId, result.Identifier);
        Assert.Equal(apiResponse.Description, result.Description);
        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetDeviceByIdAsync_ApiReturnsNotFound_ReturnsNull()
    {
        // Arrange
        var deviceId = "not-found-dev";
        _mockHttp.When(HttpMethod.Get, $"http://localhost/api/device/{deviceId}")
                 .Respond(System.Net.HttpStatusCode.NotFound);

        // Act
        var result = await _service.GetDeviceByIdAsync(deviceId);

        // Assert
        Assert.Null(result);
        _mockHttp.VerifyNoOutstandingExpectation();
    }
}