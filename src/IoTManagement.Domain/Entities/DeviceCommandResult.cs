using System;

namespace IoTManagement.Domain.Entities
{
    public class DeviceCommandResult
    {
        public int Id { get; set; }
        public string RawResult { get; set; }
        public string FormattedResult { get; set; }
        public DateTime ExecutedAt { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }

        public int DeviceCommandId { get; set; } // Foreign key to DeviceCommand.Id
        public virtual DeviceCommand DeviceCommand { get; set; } // Navigation property

        public string UserId { get; set; } // Foreign key to User.Id (string)
        public virtual User User { get; set; } // Navigation property

        public DeviceCommandResult()
        {
            ExecutedAt = DateTime.UtcNow;
        }

        public static DeviceCommandResult CreateSuccess(string rawResult, string formattedResult, DeviceCommand command, User user)
        {
            return new DeviceCommandResult
            {
                RawResult = rawResult,
                FormattedResult = formattedResult,
                Success = true,
                DeviceCommandId = command.Id,
                // DeviceCommand = command, // EF Core typically handles this if IDs are set
                UserId = user.Id,
                // User = user // EF Core typically handles this
            };
        }

        public static DeviceCommandResult CreateFailure(string errorMessage, DeviceCommand command, User user)
        {
            return new DeviceCommandResult
            {
                ErrorMessage = errorMessage,
                Success = false,
                DeviceCommandId = command.Id,
                // DeviceCommand = command,
                UserId = user.Id,
                // User = user
            };
        }
    }
}