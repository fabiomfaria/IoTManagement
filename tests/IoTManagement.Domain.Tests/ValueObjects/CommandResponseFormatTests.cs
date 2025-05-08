using Xunit;
using IoTManagement.Domain.ValueObjects;
using IoTManagement.Domain.Exceptions; // If it throws custom exceptions

public class CommandResponseFormatTests
{
    [Fact]
    public void CommandResponseFormat_Create_ValidJsonSchema_Succeeds()
    {
        // Arrange
        var validJsonSchema = "{ \"type\": \"object\", \"properties\": { \"temp\": { \"type\": \"number\" } } }";

        // Act
        var format = CommandResponseFormat.Create(validJsonSchema);

        // Assert
        Assert.Equal(validJsonSchema, format.SchemaDefinition);
    }

    [Fact]
    public void CommandResponseFormat_Create_PlainText_Succeeds()
    {
        // Arrange
        var plainTextFormat = "text/plain"; // Or just "plain_text" if that's how you define it

        // Act
        var format = CommandResponseFormat.Create(plainTextFormat);

        // Assert
        Assert.Equal(plainTextFormat, format.SchemaDefinition);
    }

    [Fact]
    public void CommandResponseFormat_Create_NullOrEmptySchema_ThrowsValidationException()
    {
        // Act & Assert
        Assert.Throws<ValidationException>(() => CommandResponseFormat.Create(null));
        Assert.Throws<ValidationException>(() => CommandResponseFormat.Create(string.Empty));
    }

    [Fact]
    public void CommandResponseFormat_Equals_SameSchema_ReturnsTrue()
    {
        // Arrange
        var schema = "{ \"type\": \"integer\" }";
        var format1 = CommandResponseFormat.Create(schema);
        var format2 = CommandResponseFormat.Create(schema);

        // Act & Assert
        Assert.Equal(format1, format2);
        Assert.True(format1 == format2);
        Assert.False(format1 != format2);
        Assert.Equal(format1.GetHashCode(), format2.GetHashCode());
    }

    [Fact]
    public void CommandResponseFormat_Equals_DifferentSchema_ReturnsFalse()
    {
        // Arrange
        var format1 = CommandResponseFormat.Create("{ \"type\": \"integer\" }");
        var format2 = CommandResponseFormat.Create("{ \"type\": \"string\" }");

        // Act & Assert
        Assert.NotEqual(format1, format2);
        Assert.False(format1 == format2);
        Assert.True(format1 != format2);
    }

    // TODO: Add more tests if CommandResponseFormat has methods for parsing/validating against the schema, etc.
    // For example, if it has a method IsJsonBased() or GetSchemaType()
}