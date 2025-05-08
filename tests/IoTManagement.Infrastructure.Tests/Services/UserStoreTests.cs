using Xunit;
using Moq; // If UserStore has dependencies, otherwise not needed for simple in-memory
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using IoTManagement.Infrastructure.Services; // Where UserStore is
using IoTManagement.Domain.Entities;       // For User

public class UserStoreTests
{
    private readonly Mock<ILogger<UserStore>> _mockLogger;
    private readonly UserStore _store;

    public UserStoreTests()
    {
        _mockLogger = new Mock<ILogger<UserStore>>();
        // Assuming UserStore takes a list of predefined users or has a way to add them.
        // For testing, we can initialize it with some users.
        // This depends heavily on UserStore's constructor and internal storage.
        // Example: If UserStore takes an IEnumerable<User> in constructor:
        var predefinedUsers = new List<User>
        {
            // Passwords should be hashed if UserStore.ValidateCredentialsAsync expects hashed passwords.
            // For simplicity, let's assume UserStore handles hashing or direct comparison for this test.
            new User { Id = "1", Username = "admin", Email="admin@iot.com", HashedPassword = UserStore.HashPassword("password123") },
            new User { Id = "2", Username = "user", Email="user@iot.com", HashedPassword = UserStore.HashPassword("securepass") }
        };
        _store = new UserStore(_mockLogger.Object, predefinedUsers);
        // If UserStore has an AddUser method:
        // _store = new UserStore(_mockLogger.Object);
        // _store.AddUser(new User { ... });
    }

    [Fact]
    public async Task FindByUsernameAsync_UserExists_ReturnsUser()
    {
        // Arrange
        var usernameToFind = "admin";

        // Act
        var user = await _store.FindByUsernameAsync(usernameToFind);

        // Assert
        Assert.NotNull(user);
        Assert.Equal(usernameToFind, user.Username);
    }

    [Fact]
    public async Task FindByUsernameAsync_UserDoesNotExist_ReturnsNull()
    {
        // Arrange
        var usernameToFind = "nonexistentuser";

        // Act
        var user = await _store.FindByUsernameAsync(usernameToFind);

        // Assert
        Assert.Null(user);
    }

    [Fact]
    public async Task ValidateCredentialsAsync_ValidCredentials_ReturnsUser()
    {
        // Arrange
        var username = "user";
        var password = "securepass";

        // Act
        var user = await _store.ValidateCredentialsAsync(username, password);

        // Assert
        Assert.NotNull(user);
        Assert.Equal(username, user.Username);
    }

    [Fact]
    public async Task ValidateCredentialsAsync_InvalidPassword_ReturnsNull()
    {
        // Arrange
        var username = "admin";
        var password = "wrongpassword";

        // Act
        var user = await _store.ValidateCredentialsAsync(username, password);

        // Assert
        Assert.Null(user);
    }

    [Fact]
    public async Task ValidateCredentialsAsync_UserNotFound_ReturnsNull()
    {
        // Arrange
        var username = "unknownuser";
        var password = "anypassword";

        // Act
        var user = await _store.ValidateCredentialsAsync(username, password);

        // Assert
        Assert.Null(user);
    }

    // Static helper for hashing if UserStore uses it publicly, or replicate internal logic for test setup.
    // This is just an example if UserStore itself doesn't expose hashing method for setup.
    // Ideally, UserStore's constructor or AddUser method would take already hashed passwords or handle it.
    /*
    private static string HashPasswordForTest(string password)
    {
        // Simple "mock" hash for testing. Real UserStore should use strong hashing.
        // In a real scenario, UserStore.ValidateCredentialsAsync would compare a hashed input password
        // against a stored hashed password.
        return $"hashed_{password}"; 
    }
    */
}