using System;

namespace IoTManagement.Domain.Exceptions
{
    public class DeviceCommandNotFoundException : Exception
    {
        public int CommandId { get; }
        public int DeviceId { get; } // This refers to Device.Id (int)

        public DeviceCommandNotFoundException(int commandId, int deviceId)
            : base($"Device command with ID {commandId} for device with ID {deviceId} was not found.")
        {
            CommandId = commandId;
            DeviceId = deviceId;
        }

        public DeviceCommandNotFoundException(string message)
            : base(message)
        {
        }

        public DeviceCommandNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}