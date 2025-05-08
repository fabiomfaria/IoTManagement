using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using IoTManagement.API.Controllers;
using IoTManagement.Application.Interfaces;
using IoTManagement.Application.DTOs; // For command execution DTOs
using IoTManagement.API.Models;     // For API specific request/response models if different

public class CommandsControllerTests
{
    private readonly Mock<ICommandService> _mockCommandService;
    private readonly Mock<ILogger<CommandsController>> _mockLogger;
    private readonly CommandsController _controller;

    public CommandsControllerTests()
    {
        _mockCommandService = new Mock<ICommandService>();
        _mockLogger = new Mock<ILogger<CommandsController>>();
        _controller = new CommandsController(_mockCommandService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task ExecuteCommand_ValidRequest_ReturnsOkWithResult()
    {
        // Arrange
        var deviceId = "dev-123";
        var commandName = "get_temp";
        // Assuming API.Models.CommandExecutionRequestModel for the request body
        var apiRequest = new CommandExecutionRequestModel
        {
            Parameters = new Dictionary<string, string> { { "unit", "celsius" } }
        };

        // And Application.DTOs.DeviceCommandExecutionResponseDto is returned by the service
        var serviceResponseDto = new DeviceCommandExecutionResponseDto
        {
            IsSuccess = true,
            Result = "25C",
            FormattedResult = "Temperature: 25 Celsius"
        };

        _mockCommandService.Setup(s => s.ExecuteCommandAsync(
                deviceId,
                commandName,
                It.Is<DeviceCommandExecutionRequestDto>(dto => dto.Parameters["unit"] == "celsius")))
            .ReturnsAsync(serviceResponseDto);

        // Act
        var result = await _controller.ExecuteCommand(deviceId, commandName, apiRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actualResponse = Assert.IsType<DeviceCommandExecutionResponseDto>(okResult.Value);
        Assert.True(actualResponse.IsSuccess);
        Assert.Equal(serviceResponseDto.Result, actualResponse.Result);
        _mockCommandService.Verify(s => s.ExecuteCommandAsync(deviceId, commandName, It.IsAny<DeviceCommandExecutionRequestDto>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteCommand_ServiceReturnsError_ReturnsProblem() // Or specific error code
    {
        // Arrange
        var deviceId = "dev-123";
        var commandName = "unknown_cmd";
        var apiRequest = new CommandExecutionRequestModel();
        var serviceResponseDto = new DeviceCommandExecutionResponseDto
        {
            IsSuccess = false,
            ErrorMessage = "Command not found"
        };

        _mockCommandService.Setup(s => s.ExecuteCommandAsync(deviceId, commandName, It.IsAny<DeviceCommandExecutionRequestDto>()))
            .ReturnsAsync(serviceResponseDto);

        // Act
        var result = await _controller.ExecuteCommand(deviceId, commandName, apiRequest);

        // Assert
        // Depending on how your controller handles errors from the service.
        // It might return BadRequest, NotFound, or a generic 500 (Problem).
        // If using ProblemDetails:
        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
        Assert.True(objectResult.StatusCode == 400 || objectResult.StatusCode == 404 || objectResult.StatusCode == 500);
        var problemDetails = Assert.IsType<ProblemDetails>(objectResult.Value);
        Assert.Contains("Command not found", problemDetails.Detail ?? problemDetails.Title);
    }

    // TODO: Add tests for device not found (404 from service layer directly or mapped by controller)
    // TODO: Add tests for invalid parameters (400 Bad Request)
    // TODO: Add tests for specific exceptions from service layer and how controller handles them
}
