using System; // Added missing using

namespace IoTManagement.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when validation errors occur
    /// </summary>
    public class ValidationException : Exception
    {
        public ValidationException() : base("A validation error has occurred") { }

        public ValidationException(string message) : base(message) { }

        // Consider adding a constructor to hold multiple validation errors
        // public IReadOnlyDictionary<string, string[]> Errors { get; }
        // public ValidationException(IReadOnlyDictionary<string, string[]> errors) : this("Multiple validation errors occurred.") { Errors = errors; }

        public ValidationException(string message, Exception innerException) : base(message, innerException) { }
    }
}