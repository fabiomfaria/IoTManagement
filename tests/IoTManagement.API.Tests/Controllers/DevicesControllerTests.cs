using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using IoTManagement.API.Controllers;
using IoTManagement.Application.Interfaces;
using IoTManagement.Application.DTOs; // Assuming DeviceDto is here
using IoTManagement.API.Models; // For DeviceModel if used directly, but prefer DTOs from Application

public class DevicesControllerTests
{
    private readonly Mock<IDeviceService> _mockDeviceService;
    private readonly Mock<ILogger<DevicesController>> _mockLogger;
    private readonly DevicesController _controller;

    public DevicesControllerTests()
    {
        _mockDeviceService = new Mock<IDeviceService>();
        _mockLogger = new Mock<ILogger<DevicesController>>();
        // Assuming your DevicesController takes ILogger, adjust if not
        _controller = new DevicesController(_mockDeviceService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetDevices_ReturnsOkObjectResult_WithListOfDeviceIdentifiers()
    {
        // Arrange
        var expectedDeviceIds = new List<string> { "device1", "device2" };
        _mockDeviceService.Setup(service => service.GetAllDeviceIdentifiersAsync())
            .ReturnsAsync(expectedDeviceIds);

        // Act
        var result = await _controller.GetDevices();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actualDeviceIds = Assert.IsAssignableFrom<IEnumerable<string>>(okResult.Value);
        Assert.Equal(expectedDeviceIds, actualDeviceIds);
        _mockDeviceService.Verify(service => service.GetAllDeviceIdentifiersAsync(), Times.Once);
    }

    [Fact]
    public async Task GetDeviceById_DeviceExists_ReturnsOkObjectResult_WithDeviceDetails()
    {
        // Arrange
        var deviceId = "test-device-id";
        // Assuming IoTManagement.Application.DTOs.DeviceDto is the detailed DTO
        var expectedDevice = new IoTApplicationDeviceDto
        {
            Identifier = deviceId,
            Description = "Test Device",
            Manufacturer = "Test Manufacturer",
            Url = "tcp://localhost:1234"
            // Commands would be populated if this DTO includes them
        };
        _mockDeviceService.Setup(service => service.GetDeviceByIdAsync(deviceId))
            .ReturnsAsync(expectedDevice);

        // Act
        var result = await _controller.GetDeviceById(deviceId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        // Assuming the controller returns IoTManagement.API.Models.DeviceModel
        // And there's a mapping or it directly uses Application DTOs
        var actualDevice = Assert.IsType<IoTApplicationDeviceDto>(okResult.Value);
        Assert.Equal(expectedDevice.Identifier, actualDevice.Identifier);
        _mockDeviceService.Verify(service => service.GetDeviceByIdAsync(deviceId), Times.Once);
    }

    [Fact]
    public async Task GetDeviceById_DeviceDoesNotExist_ReturnsNotFoundResult()
    {
        // Arrange
        var deviceId = "non-existent-device";
        _mockDeviceService.Setup(service => service.GetDeviceByIdAsync(deviceId))
            .ReturnsAsync((IoTApplicationDeviceDto)null); // Or throw a specific NotFoundException from service

        // Act
        var result = await _controller.GetDeviceById(deviceId);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
        _mockDeviceService.Verify(service => service.GetDeviceByIdAsync(deviceId), Times.Once);
    }

    [Fact]
    public async Task CreateDevice_ValidModel_ReturnsCreatedAtActionResult()
    {
        // Arrange
        // Using IoTManagement.API.Models.DeviceModel for request body
        var newDeviceModel = new DeviceModel // This is from IoTManagement.API.Models
        {
            Identifier = "new-device",
            Description = "A brand new device",
            Manufacturer = "New MFG",
            Url = "tcp://newdevice:5000",
            Commands = new List<CommandDescriptionModel>() // from API.Models
        };

        // The service layer DTO might be different
        var createdDeviceDto = new IoTApplicationDeviceDto
        {
            Identifier = newDeviceModel.Identifier,
            Description = newDeviceModel.Description,
            Manufacturer = newDeviceModel.Manufacturer,
            Url = newDeviceModel.Url
        };

        _mockDeviceService.Setup(service => service.CreateDeviceAsync(It.IsAny<IoTApplicationDeviceDto>()))
            .ReturnsAsync(createdDeviceDto); // Simulate creation returning the DTO

        // Act
        var result = await _controller.CreateDevice(newDeviceModel);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(_controller.GetDeviceById), createdAtActionResult.ActionName);
        Assert.Equal(newDeviceModel.Identifier, createdAtActionResult.RouteValues["id"]);
        var returnedDevice = Assert.IsType<IoTApplicationDeviceDto>(createdAtActionResult.Value);
        Assert.Equal(newDeviceModel.Identifier, returnedDevice.Identifier);
        _mockDeviceService.Verify(s => s.CreateDeviceAsync(It.Is<IoTApplicationDeviceDto>(d => d.Identifier == newDeviceModel.Identifier)), Times.Once);
    }

    // TODO: Add tests for UpdateDevice (PUT /device/{id})
    // TODO: Add tests for DeleteDevice (DELETE /device/{id})
    // Consider different responses: 200, 401 (auth not covered here), 404
}

using IoTApplicationDeviceDto = IoTManagement.Application.DTOs.DeviceDto;
