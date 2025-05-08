using System; // Added missing using

namespace IoTManagement.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when a user is not authorized to perform an action
    /// </summary>
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException() : base("You are not authorized to perform this action") { }

        public UnauthorizedException(string message) : base(message) { }

        public UnauthorizedException(string message, Exception innerException) : base(message, innerException) { }
    }
}