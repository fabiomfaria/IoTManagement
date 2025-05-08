using System.Collections.Generic;

namespace IoTManagement.API.Models
{
    /// <summary>
    /// DTO para representação de um dispositivo
    /// </summary>
    public class DeviceDto
    {
        public string Identifier { get; set; }
        public string Description { get; set; }
        public string Manufacturer { get; set; }
        public string Url { get; set; }
        public List<CommandDescriptionDto> Commands { get; set; } = new List<CommandDescriptionDto>();
    }

    /// <summary>
    /// DTO para representação da descrição de um comando
    /// </summary>
    public class CommandDescriptionDto
    {
        public string Operation { get; set; }
        public string Description { get; set; }
        public CommandDto Command { get; set; }
        public string Result { get; set; }
        public string Format { get; set; }
    }

    /// <summary>
    /// DTO para representação de um comando
    /// </summary>
    public class CommandDto
    {
        public string CommandText { get; set; }
        public List<ParameterDto> Parameters { get; set; } = new List<ParameterDto>();
    }

    /// <summary>
    /// DTO para representação de um parâmetro de comando
    /// </summary>
    public class ParameterDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// DTO para execução de um comando
    /// </summary>
    public class ExecuteCommandDto
    {
        public string[] ParameterValues { get; set; }
    }
}