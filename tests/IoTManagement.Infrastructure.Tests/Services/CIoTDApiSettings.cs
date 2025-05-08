using Xunit;
using Moq;
using RichardSzalay.MockHttp; // For mocking HttpClient
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using IoTManagement.Infrastructure.Services;
// Assuming API.Models are used for deserializing responses from the external API
using ApiDeviceModel = IoTManagement.API.Models.DeviceModel;
using ApiCommandDescriptionModel = IoTManagement.API.Models.CommandDescriptionModel; // etc.


// Configuration class for CIoTDApiService if it uses IOptions
public class CIoTDApiSettings
{
    public string BaseUrl { get; set; }
    public string Username { get; set; } // For Basic Auth
    public string Password { get.set; } // For Basic Auth
}

public class CIoTDApiServiceTests
{
    private readonly MockHttpMessageHandler _mockHttp;
    private readonly CIoTDApiService _service;
    private readonly Mock<ILogger<CIoTDApiService>> _mockLogger;
    private readonly CIoTDApiSettings _settings;

    public CIoTDApiServiceTests()
    {
        _mockHttp = new MockHttpMessageHandler();
        var httpClient = _mockHttp.ToHttpClient();
        
        _settings = new CIoTDApiSettings { BaseUrl = "http://fakeciotd.com/api/", Username = "user", Password = "password" };
        var mockOptions = new Mock<IOptions<CIoTDApiSettings>>();
        mockOptions.Setup(o => o.Value).Returns(_settings);

        _mockLogger = new Mock<ILogger<CIoTDApiService>>();

        // Assuming CIoTDApiService takes HttpClient, IOptions<CIoTDApiSettings>, ILogger
        _service = new CIoTDApiService(httpClient, mockOptions.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task ListDevicesAsync_ApiReturnsSuccess_ReturnsDeviceIds()
    {
        // Arrange
        var expectedDeviceIds = new List<string> { "id1", "id2" };
        var jsonResponse = JsonSerializer.Serialize(expectedDeviceIds);
        _mockHttp.When($"{_settings.BaseUrl}device")
                 .Respond("application/json", jsonResponse);

        // Act
        var result = await _service.ListDevicesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedDeviceIds.Count, result.Count);
        Assert.Equal(expectedDeviceIds, result);
        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task GetDeviceDetailsAsync_ApiReturnsDevice_ReturnsDeviceModel()
    {
        // Arrange
        var deviceId = "test-device";
        var expectedDevice = new ApiDeviceModel { Identifier = deviceId, Description = "Test" };
        var jsonResponse = JsonSerializer.Serialize(expectedDevice);
        _mockHttp.When($"{_settings.BaseUrl}device/{deviceId}")
                 .Respond("application/json", jsonResponse);

        // Act
        var result = await _service.GetDeviceDetailsAsync(deviceId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(deviceId, result.Identifier);
        Assert.Equal(expectedDevice.Description, result.Description);
        _mockHttp.VerifyNoOutstandingExpectation();
    }
    
    [Fact]
    public async Task GetDeviceDetailsAsync_ApiReturnsNotFound_ReturnsNull()
    {
        // Arrange
        var deviceId = "not-found-device";
        _mockHttp.When($"{_settings.BaseUrl}device/{deviceId}")
                 .Respond(HttpStatusCode.NotFound);

        // Act
        var result = await _service.GetDeviceDetailsAsync(deviceId);

        // Assert
        Assert.Null(result);
        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task CreateDeviceAsync_ValidDevice_SendsPostRequest_ReturnsTaskCompleted()
    {
        // Arrange
        var deviceToCreate = new ApiDeviceModel
        {
            Identifier = "new-device",
            Description = "A new device",
            Manufacturer = "MFG",
            Url = "tcp://new.iot:1234"
            // Commands can be added here if needed for serialization
        };
        var expectedJsonPayload = JsonSerializer.Serialize(deviceToCreate);

        _mockHttp.When(HttpMethod.Post, $"{_settings.BaseUrl}device")
                 .WithContent(expectedJsonPayload) // Verify the content sent
                 .Respond(HttpStatusCode.Created, 
                    new Dictionary<string, string> { { "Location", $"{_settings.BaseUrl}device/{deviceToCreate.Identifier}" } }, // Headers
                    new StringContent("")); // Empty content for 201

        // Act
        await _service.CreateDeviceAsync(deviceToCreate); // Assuming it's Task (void)

        // Assert
        _mockHttp.VerifyNoOutstandingExpectation(); // Verifies the POST request was made as expected
    }

    [Fact]
    public async Task CreateDeviceAsync_ApiReturnsConflict_ThrowsHttpRequestException()
    {
        // Arrange
        var deviceToCreate = new ApiDeviceModel { Identifier = "existing-device" };
         _mockHttp.When(HttpMethod.Post, $"{_settings.BaseUrl}device")
                 .Respond(HttpStatusCode.Conflict); // Or any other error status

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _service.CreateDeviceAsync(deviceToCreate));
        _mockHttp.VerifyNoOutstandingExpectation();
    }

    // TODO: Tests for UpdateDeviceAsync (PUT)
    // TODO: Tests for DeleteDeviceAsync (DELETE)
    // TODO: Tests for authentication (Basic Auth header should be present on requests if CIoTD API requires it)
    // Example for checking Basic Auth header:
    // _mockHttp.When($"{_settings.BaseUrl}device")
    //          .WithHeaders("Authorization", $"Basic {Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{_settings.Username}:{_settings.Password}"))}")
    //          .Respond("application/json", "[]");
}