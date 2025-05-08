using System.Collections.Generic;

namespace IoTManagement.Application.DTOs
{
    public class DeviceDto
    {
        public string Id { get; set; }
        public string Manufacturer { get; set; }
        public string Description { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public List<DeviceCommandDto> Commands { get; set; } = new List<DeviceCommandDto>();
    }

    public class DeviceListItemDto
    {
        public string Id { get; set; }
        public string Manufacturer { get; set; }
        public string Description { get; set; }
    }
}