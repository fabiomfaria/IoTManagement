using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IoTManagement.API.Models
{
    /// <summary>
    /// Representação de um dispositivo para a API
    /// </summary>
    public class DeviceModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Manufacturer { get; set; }
        public string IpAddress { get; set; }
        public int Port { get; set; }
        public List<CommandModel> Commands { get; set; } = new List<CommandModel>();
    }

    /// <summary>
    /// Representação de um comando de dispositivo para a API
    /// </summary>
    public class CommandModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ResultDescription { get; set; }
        public CommandDetailsModel CommandDetails { get; set; }
        public ResponseFormatModel Format { get; set; }
    }

    /// <summary>
    /// Detalhes de um comando
    /// </summary>
    public class CommandDetailsModel
    {
        public string Command { get; set; }
        public List<ParameterModel> Parameters { get; set; } = new List<ParameterModel>();
    }

    /// <summary>
    /// Representação de um parâmetro de comando
    /// </summary>
    public class ParameterModel
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public bool Required { get; set; }
    }

    /// <summary>
    /// Representação do formato de resposta de um comando
    /// </summary>
    public class ResponseFormatModel
    {
        public string Description { get; set; }
        public string Format { get; set; }
    }

    /// <summary>
    /// Requisição para execução de um comando
    /// </summary>
    public class CommandExecutionRequest
    {
        [Required]
        public string DeviceId { get; set; }
        
        [Required]
        public string CommandId { get; set; }
        
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Resposta da execução de um comando
    /// </summary>
    public class CommandExecutionResponse
    {
        public string Result { get; set; }
        public string FormattedResult { get; set; }
        public string FormatDescription { get; set; }
    }
}