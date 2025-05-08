using System.Collections.Generic;

namespace IoTManagement.Application.DTOs
{
    public class DeviceCommandDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ResultDescription { get; set; }
        public string CommandText { get; set; }
        public string ResponseFormat { get; set; }
        public List<DeviceCommandParameterDto> Parameters { get; set; } = new List<DeviceCommandParameterDto>();
    }

    public class DeviceCommandParameterDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public bool Required { get; set; }
        public string DefaultValue { get; set; }
    }
}