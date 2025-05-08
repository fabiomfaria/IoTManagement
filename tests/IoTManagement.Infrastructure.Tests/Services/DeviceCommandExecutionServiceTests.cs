using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;
using IoTManagement.Infrastructure.Services;
using IoTManagement.Domain.Services;   // For ITelnetClient, IDeviceCommandExecutionService
using IoTManagement.Domain.Entities;
using IoTManagement.Domain.ValueObjects; // For CommandResponseFormat
using IoTManagement.Domain.Exceptions;

public class DeviceCommandExecutionServiceTests
{
    private readonly Mock<ITelnetClient> _mockTelnetClient;
    private readonly Mock<ILogger<DeviceCommandExecutionService>> _mockLogger;
    private readonly DeviceCommandExecutionService _service;

    public DeviceCommandExecutionServiceTests()
    {
        _mockTelnetClient = new Mock<ITelnetClient>();
        _mockLogger = new Mock<ILogger<DeviceCommandExecutionService>>();
        _service = new DeviceCommandExecutionService(_mockTelnetClient.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteCommandAsync_SuccessfulTelnetCall_ReturnsSuccessResult()
    {
        // Arrange
        var device = new Device { Identifier = "d1", Url = "telnet://localhost:123" }; // Assuming URL is parsed by service
        var command = new DeviceCommand
        {
            Command = "GET_DATA",
            Parameters = new List<DeviceCommandParameter> { new DeviceCommandParameter { Name = "id" } },
            ResultFormat = CommandResponseFormat.Create("{\"type\":\"object\",\"properties\":{\"value\":{\"type\":\"string\"}}}")
        };
        var parameters = new Dictionary<string, string> { { "id", "sensor1" } };
        var telnetResponse = "{\"value\":\"rawData\"}";

        _mockTelnetClient.Setup(t => t.SendCommandAsync("localhost", 123, "GET_DATA", It.Is<IEnumerable<string>>(p => p.Contains("sensor1"))))
            .ReturnsAsync(telnetResponse);

        // Act
        var result = await _service.ExecuteCommandAsync(device, command, parameters);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(telnetResponse, result.RawOutput);
        // Assuming FormattedOutput might be the same as RawOutput if no complex formatting logic is in place for this test
        Assert.Equal(telnetResponse, result.FormattedOutput);
        _mockTelnetClient.Verify(t => t.SendCommandAsync("localhost", 123, "GET_DATA", It.Is<IEnumerable<string>>(p => p.First() == "sensor1")), Times.Once);
    }

    [Fact]
    public async Task ExecuteCommandAsync_TelnetCallFails_ReturnsErrorResult()
    {
        // Arrange
        var device = new Device { Identifier = "d1", Url = "telnet://host.invalid:123" };
        var command = new DeviceCommand { Command = "FAIL_CMD" };
        var parameters = new Dictionary<string, string>();

        _mockTelnetClient.Setup(t => t.SendCommandAsync("host.invalid", 123, "FAIL_CMD", It.IsAny<IEnumerable<string>>()))
            .ThrowsAsync(new TelnetCommunicationException("Connection failed"));

        // Act
        var result = await _service.ExecuteCommandAsync(device, command, parameters);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Connection failed", result.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteCommandAsync_InvalidDeviceUrl_ReturnsErrorResult()
    {
        // Arrange
        var device = new Device { Identifier = "d1", Url = "invalid-url-format" };
        var command = new DeviceCommand { Command = "ANY_CMD" };

        // Act
        var result = await _service.ExecuteCommandAsync(device, command, new Dictionary<string, string>());

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Invalid device URL format", result.ErrorMessage);
        _mockTelnetClient.Verify(t => t.SendCommandAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteCommandAsync_MissingRequiredParameter_ReturnsErrorResult()
    {
        // Arrange
        var device = new Device { Identifier = "d1", Url = "telnet://localhost:123" };
        var command = new DeviceCommand
        {
            Command = "CMD_WITH_PARAM",
            // Parameter "req_param" is implicitly required if it's in the list (depends on your design)
            // Or you might have an IsRequired flag on DeviceCommandParameter
            Parameters = new List<DeviceCommandParameter> { new DeviceCommandParameter { Name = "req_param" /*, IsRequired = true */ } }
        };
        var parameters = new Dictionary<string, string>(); // Missing "req_param"

        // Act
        var result = await _service.ExecuteCommandAsync(device, command, parameters);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains("Missing required parameter: req_param", result.ErrorMessage);
    }

    // TODO: Test parameter ordering if it matters for Telnet command string
    // TODO: Test formatting logic if FormattedOutput is different from RawOutput based on ResultFormat
}