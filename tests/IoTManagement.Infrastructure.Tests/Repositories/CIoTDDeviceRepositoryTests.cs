using Xunit;
using Moq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using IoTManagement.Infrastructure.Repositories;
// Assuming ICIoTDApiService is what CIoTDDeviceRepository uses to fetch data.
// This interface is defined in IoTManagement.API.Interfaces
using IoTManagement.API.Interfaces;
// The repository should return Domain entities/models
using IoTManagement.Domain.Entities;
// The DTOs used by ICIoTDApiService might be from API.Models or specific to it.
// Let's assume ICIoTDApiService returns something like API.Models.Device
using ApiDeviceModel = IoTManagement.API.Models.DeviceModel; 
using ApiCommandDescriptionModel = IoTManagement.API.Models.CommandDescriptionModel;
using ApiCommandModel = IoTManagement.API.Models.CommandModel;
using ApiParameterModel = IoTManagement.API.Models.ParameterModel;


public class CIoTDDeviceRepositoryTests
{
    private readonly Mock<ICIoTDApiService> _mockCIoTDApiService;
    private readonly Mock<ILogger<CIoTDDeviceRepository>> _mockLogger;
    // private readonly Mock<IMapper> _mockMapper; // If AutoMapper is used for DTO -> Entity
    private readonly CIoTDDeviceRepository _repository;

    public CIoTDDeviceRepositoryTests()
    {
        _mockCIoTDApiService = new Mock<ICIoTDApiService>();
        _mockLogger = new Mock<ILogger<CIoTDDeviceRepository>>();
        // _mockMapper = new Mock<IMapper>();
        
        // Adjust constructor if it uses a mapper
        _repository = new CIoTDDeviceRepository(_mockCIoTDApiService.Object, _mockLogger.Object /*, _mockMapper.Object */);
    }

    [Fact]
    public async Task GetByIdAsync_ApiReturnsDevice_ReturnsMappedDomainDevice()
    {
        // Arrange
        var deviceId = "test-id";
        var apiDeviceModel = new ApiDeviceModel
        {
            Identifier = deviceId,
            Description = "API Device Desc",
            Manufacturer = "API MFG",
            Url = "tcp://api.device:1234",
            Commands = new List<ApiCommandDescriptionModel>
            {
                new ApiCommandDescriptionModel
                {
                    Operation = "getStatus",
                    Description = "Gets status",
                    Command = new ApiCommandModel { Command = "STATUS", Parameters = new List<ApiParameterModel>()},
                    Result = "Device status",
                    Format = "text/plain"
                }
            }
        };
        _mockCIoTDApiService.Setup(s => s.GetDeviceDetailsAsync(deviceId)).ReturnsAsync(apiDeviceModel);

        // If using IMapper
        // var expectedDomainDevice = new Device { Identifier = deviceId, ... };
        // _mockMapper.Setup(m => m.Map<Device>(apiDeviceModel)).Returns(expectedDomainDevice);

        // Act
        var result = await _repository.GetByIdAsync(deviceId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(deviceId, result.Identifier);
        Assert.Equal(apiDeviceModel.Description, result.Description); // Assuming direct mapping for test
        Assert.Equal(apiDeviceModel.Manufacturer, result.Manufacturer);
        Assert.Equal(apiDeviceModel.Url, result.Url);
        Assert.Single(result.Commands);
        Assert.Equal("getStatus", result.Commands.First().Operation);

        _mockCIoTDApiService.Verify(s => s.GetDeviceDetailsAsync(deviceId), Times.Once);
        // _mockMapper.Verify(m => m.Map<Device>(apiDeviceModel), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ApiReturnsNull_ReturnsNull()
    {
        // Arrange
        var deviceId = "not-found-id";
        _mockCIoTDApiService.Setup(s => s.GetDeviceDetailsAsync(deviceId)).ReturnsAsync((ApiDeviceModel)null);

        // Act
        var result = await _repository.GetByIdAsync(deviceId);

        // Assert
        Assert.Null(result);
        _mockCIoTDApiService.Verify(s => s.GetDeviceDetailsAsync(deviceId), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ApiReturnsDeviceList_ReturnsMappedDomainDevices()
    {
        // Arrange
        var apiDeviceIds = new List<string> { "id1", "id2" };
        var apiDevice1 = new ApiDeviceModel { Identifier = "id1", Description = "Device 1" };
        var apiDevice2 = new ApiDeviceModel { Identifier = "id2", Description = "Device 2" };

        _mockCIoTDApiService.Setup(s => s.ListDevicesAsync()).ReturnsAsync(apiDeviceIds);
        _mockCIoTDApiService.Setup(s => s.GetDeviceDetailsAsync("id1")).ReturnsAsync(apiDevice1);
        _mockCIoTDApiService.Setup(s => s.GetDeviceDetailsAsync("id2")).ReturnsAsync(apiDevice2);
        
        // If using IMapper for mapping ApiDeviceModel to Domain.Device
        // _mockMapper.Setup(m => m.Map<Device>(apiDevice1)).Returns(new Device { Identifier = "id1", Description = "Device 1" });
        // _mockMapper.Setup(m => m.Map<Device>(apiDevice2)).Returns(new Device { Identifier = "id2", Description = "Device 2" });


        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, d => d.Identifier == "id1");
        Assert.Contains(result, d => d.Identifier == "id2");

        _mockCIoTDApiService.Verify(s => s.ListDevicesAsync(), Times.Once);
        _mockCIoTDApiService.Verify(s => s.GetDeviceDetailsAsync("id1"), Times.Once);
        _mockCIoTDApiService.Verify(s => s.GetDeviceDetailsAsync("id2"), Times.Once);
        // _mockMapper.Verify(m => m.Map<Device>(It.IsAny<ApiDeviceModel>()), Times.Exactly(2));
    }

    [Fact]
    public async Task AddAsync_CallsApiServiceCreateDevice()
    {
        // Arrange
        var domainDevice = new Device // Domain.Entities.Device
        {
            Identifier = "new-dev",
            Description = "New Domain Device",
            Manufacturer = "Domain MFG",
            Url = "tcp://domain.new:7890",
            Commands = new List<DeviceCommand>() // Domain.Entities.DeviceCommand
        };

        // The repository needs to map the Domain.Device to an ApiDeviceModel
        // or whatever model ICIoTDApiService.CreateDeviceAsync expects.
        // Let's assume it expects ApiDeviceModel.

        // If using IMapper for Device -> ApiDeviceModel
        // var apiModelToCreate = new ApiDeviceModel { Identifier = "new-dev", ...};
        // _mockMapper.Setup(m => m.Map<ApiDeviceModel>(domainDevice)).Returns(apiModelToCreate);

        _mockCIoTDApiService.Setup(s => s.CreateDeviceAsync(It.Is<ApiDeviceModel>(ad => ad.Identifier == domainDevice.Identifier)))
            .Returns(Task.CompletedTask); // Assuming void or Task return

        // Act
        await _repository.AddAsync(domainDevice);

        // Assert
        _mockCIoTDApiService.Verify(s => s.CreateDeviceAsync(
            It.Is<ApiDeviceModel>(ad => 
                ad.Identifier == domainDevice.Identifier &&
                ad.Description == domainDevice.Description // etc.
            )), Times.Once);
        // _mockMapper.Verify(m => m.Map<ApiDeviceModel>(domainDevice), Times.Once);
    }
    
    // TODO: Tests for UpdateAsync, DeleteAsync if they exist and call corresponding ICIoTDApiService methods
}