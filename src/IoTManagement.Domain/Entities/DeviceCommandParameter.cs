namespace IoTManagement.Domain.Entities
{
    /// <summary>
    /// Represents a parameter for a DeviceCommand managed by our system.
    /// </summary>
    public class DeviceCommandParameter
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; } // e.g., "string", "int", "bool"
        public bool IsRequired { get; set; }
        public string DefaultValue { get; set; }

        public int DeviceCommandId { get; set; } // Foreign key to DeviceCommand.Id
        public virtual DeviceCommand DeviceCommand { get; set; } // Navigation property
    }
}