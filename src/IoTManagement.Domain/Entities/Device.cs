using System.Collections.Generic;

namespace IoTManagement.Domain.Entities
{
    /// <summary>
    /// Representa um dispositivo IoT na plataforma.
    /// </summary>
    public class Device
    {
        /// <summary>
        /// Primary key for internal database use.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Unique business identifier for the device (e.g., from CIoTD API).
        /// </summary>
        public string Identifier { get; set; }

        public string Description { get; set; }
        public string Manufacturer { get; set; }
        public string Url { get; set; }

        /// <summary>
        /// List of command descriptions available for this device, typically from its specification.
        /// </summary>
        public List<CommandDescription> Commands { get; set; } = new List<CommandDescription>();
    }

    /// <summary>
    /// Representa a descrição de um comando disponível em um dispositivo.
    /// </summary>
    public class CommandDescription
    {
        /// <summary>
        /// Name of the operation. Should map to DeviceCommand.Name or similar.
        /// </summary>
        public string Operation { get; set; }
        public string Description { get; set; }
        public Command Command { get; set; }
        public string Result { get; set; } // Expected result description
        public string Format { get; set; } // Data format of the result
    }

    /// <summary>
    /// Representa a estrutura de um comando que pode ser executado em um dispositivo,
    /// conforme definido pela especificação do dispositivo.
    /// </summary>
    public class Command
    {
        /// <summary>
        /// The base command string/template.
        /// </summary>
        public string CommandText { get; set; }
        public List<Parameter> Parameters { get; set; } = new List<Parameter>();
    }

    /// <summary>
    /// Representa um parâmetro para um comando de dispositivo,
    /// conforme definido pela especificação do comando do dispositivo.
    /// </summary>
    public class Parameter
    {
        public string Name { get; set; }
        public string Description { get; set; }
        // public string Type {get; set;} // Consider adding type if needed for validation/input
    }
}