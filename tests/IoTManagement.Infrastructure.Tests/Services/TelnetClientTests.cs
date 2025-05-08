using Xunit;
using Moq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using IoTManagement.Infrastructure.Services; // Where TelnetClient is
using IoTManagement.Domain.Services;      // Where ITelnetClient interface is
using IoTManagement.Domain.Exceptions;    // For TelnetCommunicationException

// You would need to create these abstractions if TelnetClient uses TcpClient directly
public interface ITcpClientWrapper : IDisposable
{
    bool Connected { get; }
    Task ConnectAsync(string hostname, int port);
    Stream GetStream();
    void Close();
}

public interface INetworkStreamWrapper : IDisposable // Not strictly needed if GetStream returns Stream
{
    Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken);
    Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken);
    bool DataAvailable { get; } // Useful for some Telnet interactions
}


public class TelnetClientTests
{
    private readonly Mock<ILogger<TelnetClient>> _mockLogger;
    // To truly unit test, you'd mock the actual socket/TCP client operations.
    // This requires TelnetClient to be designed for testability, e.g., by injecting a factory for TcpClient
    // or by wrapping TcpClient and NetworkStream. For this example, I'll mock a wrapper.
    private readonly Mock<ITcpClientWrapperFactory> _mockTcpClientFactory; // Assume a factory pattern
    private readonly Mock<ITcpClientWrapper> _mockTcpClient;
    private readonly Mock<Stream> _mockNetworkStream; // Using Mock<Stream> as NetworkStream is a Stream

    public interface ITcpClientWrapperFactory
    {
        ITcpClientWrapper Create();
    }

    public TelnetClientTests()
    {
        _mockLogger = new Mock<ILogger<TelnetClient>>();
        _mockTcpClientFactory = new Mock<ITcpClientWrapperFactory>();
        _mockTcpClient = new Mock<ITcpClientWrapper>();
        _mockNetworkStream = new Mock<Stream>(); // Mocking Stream directly

        // Setup factory to return the mocked TcpClient
        _mockTcpClientFactory.Setup(f => f.Create()).Returns(_mockTcpClient.Object);

        // Setup TcpClient to return the mocked NetworkStream
        _mockTcpClient.Setup(c => c.GetStream()).Returns(_mockNetworkStream.Object);
        _mockTcpClient.Setup(c => c.Connected).Returns(true); // Assume connected after ConnectAsync

        // Make stream readable and writable for mock setups
        _mockNetworkStream.Setup(s => s.CanRead).Returns(true);
        _mockNetworkStream.Setup(s => s.CanWrite).Returns(true);
    }

    private TelnetClient CreateClient()
    {
        // Pass the factory to TelnetClient's constructor
        return new TelnetClient(_mockLogger.Object, _mockTcpClientFactory.Object);
    }

    [Fact]
    public async Task SendCommandAsync_SuccessfulCommunication_ReturnsResponse()
    {
        // Arrange
        var client = CreateClient();
        var host = "localhost";
        var port = 1234;
        var command = "GET_STATUS";
        var parameters = new List<string> { "param1", "param2" };
        var expectedTelnetCommand = "GET_STATUS param1 param2\r";
        var expectedResponse = "STATUS_OK\r";

        byte[] requestBytes = Encoding.ASCII.GetBytes(expectedTelnetCommand);
        byte[] responseBytes = Encoding.ASCII.GetBytes(expectedResponse);

        // Simulate connection
        _mockTcpClient.Setup(c => c.ConnectAsync(host, port)).Returns(Task.CompletedTask);

        // Simulate writing to stream
        _mockNetworkStream.Setup(s => s.WriteAsync(It.Is<byte[]>(b => Encoding.ASCII.GetString(b) == expectedTelnetCommand), 0, requestBytes.Length, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        // Simulate reading from stream
        // This is tricky. ReadAsync reads into a buffer. We need to simulate data being put into that buffer.
        _mockNetworkStream.Setup(s => s.ReadAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Callback<byte[], int, int, CancellationToken>((buffer, offset, count, token) => {
                // Copy our simulated response into the buffer provided by TelnetClient's ReadAsync call
                // Only copy up to 'count' bytes or responseBytes.Length, whichever is smaller
                int bytesToCopy = Math.Min(count, responseBytes.Length);
                Array.Copy(responseBytes, 0, buffer, offset, bytesToCopy);
            })
            .ReturnsAsync((byte[] buffer, int offset, int count, CancellationToken token) => {
                 // Return the number of bytes "read" (copied)
                return Math.Min(count, responseBytes.Length);
            });


        // Act
        var result = await client.SendCommandAsync(host, port, command, parameters);

        // Assert
        Assert.Equal("STATUS_OK", result.TrimEnd('\r')); // TelnetClient should strip trailing \r
        _mockTcpClient.Verify(c => c.ConnectAsync(host, port), Times.Once);
        _mockNetworkStream.Verify(s => s.WriteAsync(It.Is<byte[]>(b => Encoding.ASCII.GetString(b) == expectedTelnetCommand), 0, requestBytes.Length, It.IsAny<CancellationToken>()), Times.Once);
        _mockNetworkStream.Verify(s => s.ReadAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce); // May read in chunks
        _mockTcpClient.Verify(c => c.Close(), Times.Once); // Ensure connection is closed
    }

    [Fact]
    public async Task SendCommandAsync_ConnectionFails_ThrowsTelnetCommunicationException()
    {
        // Arrange
        var client = CreateClient();
        _mockTcpClient.Setup(c => c.ConnectAsync(It.IsAny<string>(), It.IsAny<int>()))
                      .ThrowsAsync(new System.Net.Sockets.SocketException());

        // Act & Assert
        await Assert.ThrowsAsync<TelnetCommunicationException>(() => 
            client.SendCommandAsync("host", 123, "cmd", new List<string>()));
        _mockTcpClient.Verify(c => c.Close(), Times.Never); // Should not attempt to close if connect failed
    }

    [Fact]
    public async Task SendCommandAsync_WriteFails_ThrowsTelnetCommunicationException()
    {
        // Arrange
        var client = CreateClient();
        _mockTcpClient.Setup(c => c.ConnectAsync(It.IsAny<string>(), It.IsAny<int>())).Returns(Task.CompletedTask);
        _mockNetworkStream.Setup(s => s.WriteAsync(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                          .ThrowsAsync(new IOException());
        
        // Act & Assert
        await Assert.ThrowsAsync<TelnetCommunicationException>(() => 
            client.SendCommandAsync("host", 123, "cmd", new List<string>()));
        _mockTcpClient.Verify(c => c.Close(), Times.Once); // Should close on error after connect
    }
}