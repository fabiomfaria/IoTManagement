using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;
using IoTManagement.Application.Services;
using IoTManagement.Application.DTOs;
using IoTManagement.Domain.Repositories; // For IDeviceRepository, IDeviceCommandRepository
using IoTManagement.Domain.Services;   // For IDeviceCommandExecutionService
using IoTManagement.Domain.Entities;
using IoTManagement.Domain.Exceptions;

public class CommandServiceTests
{
    private readonly Mock<IDeviceRepository> _mockDeviceRepository;
    private readonly Mock<IDeviceCommandRepository> _mockDeviceCommandRepository; // If used to fetch command definitions
    private readonly Mock<IDeviceCommandExecutionService> _mockExecutionService;
    private readonly Mock<ILogger<CommandService>> _mockLogger;
    private readonly CommandService _service;

    public CommandServiceTests()
    {
        _mockDeviceRepository = new Mock<IDeviceRepository>();
        _mockDeviceCommandRepository = new Mock<IDeviceCommandRepository>();
        _mockExecutionService = new Mock<IDeviceCommandExecutionService>();
        _mockLogger = new Mock<ILogger<CommandService>>();
        _service = new CommandService(
            _mockDeviceRepository.Object,
            _mockDeviceCommandRepository.Object, // Or remove if not used
            _mockExecutionService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteCommandAsync_ValidCommand_ReturnsSuccessResponse()
    {
        // Arrange
        var deviceId = "dev1";
        var commandOperationName = "getTemp";
        var requestDto = new DeviceCommandExecutionRequestDto { Parameters = new Dictionary<string, string> { { "unit", "C" } } };

        var device = new Device { Identifier = deviceId, Url = "telnet://localhost:23" };
        var commandToExecute = new DeviceCommand
        {
            Operation = commandOperationName,
            Command = "READ_TEMP",
            Parameters = new List<DeviceCommandParameter> { new DeviceCommandParameter { Name = "unit" } },
            ResultFormat = "{ \"temperature\": \"{0}\" }" // Example format
        };
        device.Commands = new List<DeviceCommand> { commandToExecute }; // Add command to device

        var executionResult = new DeviceCommandResult { RawOutput = "25", FormattedOutput = "{\"temperature\":\"25\"}", IsSuccess = true };

        _mockDeviceRepository.Setup(r => r.GetByIdAsync(deviceId)).ReturnsAsync(device);
        // If command is directly on device entity:
        // No need for _mockDeviceCommandRepository in this specific path.
        // Else if command definitions are fetched separately:
        // _mockDeviceCommandRepository.Setup(r => r.GetByDeviceAndOperationAsync(deviceId, commandOperationName)).ReturnsAsync(commandToExecute);

        _mockExecutionService.Setup(s => s.ExecuteCommandAsync(device, commandToExecute, requestDto.Parameters))
            .ReturnsAsync(executionResult);

        // Act
        var response = await _service.ExecuteCommandAsync(deviceId, commandOperationName, requestDto);

        // Assert
        Assert.True(response.IsSuccess);
        Assert.Equal(executionResult.RawOutput, response.Result);
        Assert.Equal(executionResult.FormattedOutput, response.FormattedResult);
        _mockDeviceRepository.Verify(r => r.GetByIdAsync(deviceId), Times.Once);
        _mockExecutionService.Verify(s => s.ExecuteCommandAsync(device, commandToExecute, requestDto.Parameters), Times.Once);
    }

    [Fact]
    public async Task ExecuteCommandAsync_DeviceNotFound_ReturnsErrorResponse()
    {
        // Arrange
        var deviceId = "unknownDev";
        _mockDeviceRepository.Setup(r => r.GetByIdAsync(deviceId)).ReturnsAsync((Device)null);

        // Act
        var response = await _service.ExecuteCommandAsync(deviceId, "anyCmd", new DeviceCommandExecutionRequestDto());

        // Assert
        Assert.False(response.IsSuccess);
        Assert.Contains("Device not found", response.ErrorMessage);
    }

    [Fact]
    public async Task ExecuteCommandAsync_CommandNotFoundOnDevice_ReturnsErrorResponse()
    {
        // Arrange
        var deviceId = "dev1";
        var commandOperationName = "nonExistentCmd";
        var device = new Device { Identifier = deviceId, Url = "telnet://localhost:23", Commands = new List<DeviceCommand>() }; // No commands
        _mockDeviceRepository.Setup(r => r.GetByIdAsync(deviceId)).ReturnsAsync(device);

        // Act
        var response = await _service.ExecuteCommandAsync(deviceId, commandOperationName, new DeviceCommandExecutionRequestDto());

        // Assert
        Assert.False(response.IsSuccess);
        Assert.Contains("Command not found", response.ErrorMessage); // Or specific "on device"
    }

    [Fact]
    public async Task ExecuteCommandAsync_ExecutionFails_ReturnsErrorResponseFromExecutionService()
    {
        // Arrange
        var deviceId = "dev1";
        var commandOperationName = "faultyCmd";
        var requestDto = new DeviceCommandExecutionRequestDto();
        var device = new Device { Identifier = deviceId, Url = "telnet://localhost:23" };
        var commandToExecute = new DeviceCommand { Operation = commandOperationName, Command = "FAULT" };
        device.Commands = new List<DeviceCommand> { commandToExecute };

        var executionResult = new DeviceCommandResult { IsSuccess = false, ErrorMessage = "Telnet connection failed" };

        _mockDeviceRepository.Setup(r => r.GetByIdAsync(deviceId)).ReturnsAsync(device);
        _mockExecutionService.Setup(s => s.ExecuteCommandAsync(device, commandToExecute, requestDto.Parameters))
            .ReturnsAsync(executionResult);

        // Act
        var response = await _service.ExecuteCommandAsync(deviceId, commandOperationName, requestDto);

        // Assert
        Assert.False(response.IsSuccess);
        Assert.Equal(executionResult.ErrorMessage, response.ErrorMessage);
    }

    // TODO: Add tests for invalid parameters (e.g., missing required parameter)
}
