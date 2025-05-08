using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IoTManagement.API.Models
{
    /// <summary>
    /// Modelo para detalhes de execução de comando
    /// </summary>
    public class CommandDetailsRequest
    {
        [Required]
        public string DeviceId { get; set; }
        
        [Required]
        public string CommandId { get; set; }
    }

    /// <summary>
    /// Modelo para validação de parâmetros de comando
    /// </summary>
    public class CommandParameterValidation
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public bool IsValid { get; set; }
        public string ValidationMessage { get; set; }
    }

    /// <summary>
    /// Modelo para histórico de execução de comandos
    /// </summary>
    public class CommandExecutionHistory
    {
        public string Id { get; set; }
        public string DeviceId { get; set; }
        public string CommandId { get; set; }
        public string CommandName { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
        public string Result { get; set; }
        public string FormattedResult { get; set; }
        public DateTime ExecutionTime { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// Modelo para estatísticas de execução de comandos
    /// </summary>
    public class CommandExecutionStats
    {
        public string DeviceId { get; set; }
        public string CommandId { get; set; }
        public int TotalExecutions { get; set; }
        public int SuccessfulExecutions { get; set; }
        public int FailedExecutions { get; set; }
        public double AverageExecutionTimeMs { get; set; }
        public DateTime LastExecution { get; set; }
    }

    /// <summary>
    /// Modelo para filtro de comandos
    /// </summary>
    public class CommandFilter
    {
        public string DeviceId { get; set; }
        public string NameContains { get; set; }
        public string DescriptionContains { get; set; }
        public bool? HasParameters { get; set; }
        public int? MaxParameterCount { get; set; }
        public string ResultFormatContains { get; set; }
    }
}