using System.Collections.Generic;

namespace IoTManagement.Application.DTOs
{
    public class DeviceCommandExecutionRequestDto
    {
        public string DeviceId { get; set; }
        public string CommandId { get; set; }
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
    }
}