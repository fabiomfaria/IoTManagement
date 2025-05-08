using System; // Added missing using

namespace IoTManagement.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when there is an error communicating with a device via Telnet
    /// </summary>
    public class TelnetCommunicationException : Exception
    {
        public TelnetCommunicationException() : base("Error communicating with device via Telnet") { }

        public TelnetCommunicationException(string message) : base(message) { }

        public TelnetCommunicationException(string message, Exception innerException) : base(message, innerException) { }
    }
}