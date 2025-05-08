using Xunit;
using IoTManagement.Domain.Entities;
using System.Collections.Generic;
using System.Linq;

public class DeviceTests
{
    [Fact]
    public void Device_CanBeCreated_WithValidProperties()
    {
        // Arrange
        var identifier = "device-001";
        var description = "Test Device";
        var manufacturer = "Test Manufacturer";
        var url = "telnet://localhost:12345";
        var commands = new List<DeviceCommand>
        {
            new DeviceCommand { Operation = "getStatus", Command = "STATUS", ResultFormat = "text" }
        };

        // Act
        var device = new Device
        {
            Identifier = identifier,
            Description = description,
            Manufacturer = manufacturer,
            Url = url,
            Commands = commands
        };

        // Assert
        Assert.Equal(identifier, device.Identifier);
        Assert.Equal(description, device.Description);
        Assert.Equal(manufacturer, device.Manufacturer);
        Assert.Equal(url, device.Url);
        Assert.Single(device.Commands);
        Assert.Equal("getStatus", device.Commands.First().Operation);
    }

    [Fact]
    public void FindCommand_CommandExists_ReturnsCommand()
    {
        // Arrange
        var commandToFind = new DeviceCommand { Operation = "getTemp", Command = "TEMP?" };
        var device = new Device
        {
            Identifier = "d1",
            Commands = new List<DeviceCommand>
            {
                new DeviceCommand { Operation = "getStatus", Command = "STATUS" },
                commandToFind
            }
        };

        // Act
        var foundCommand = device.FindCommand("getTemp");

        // Assert
        Assert.NotNull(foundCommand);
        Assert.Same(commandToFind, foundCommand); // Verifies it's the same instance
        Assert.Equal("getTemp", foundCommand.Operation);
    }

    [Fact]
    public void FindCommand_CommandDoesNotExist_ReturnsNull()
    {
        // Arrange
        var device = new Device
        {
            Identifier = "d1",
            Commands = new List<DeviceCommand>
            {
                new DeviceCommand { Operation = "getStatus", Command = "STATUS" }
            }
        };

        // Act
        var foundCommand = device.FindCommand("nonExistentCommand");

        // Assert
        Assert.Null(foundCommand);
    }

    // TODO: Add more tests if Device entity has methods with business logic
    // e.g., AddCommand, RemoveCommand, UpdateDetails, methods that change state or enforce invariants.
}
