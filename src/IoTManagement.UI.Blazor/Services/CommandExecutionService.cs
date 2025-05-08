using System;
using System.Text.Json;
using System.Threading.Tasks;
using IoTManagement.UI.Blazor.Models;

namespace IoTManagement.UI.Blazor.Services
{
    public class CommandExecutionService
    {
        private readonly DeviceService _deviceService;

        public CommandExecutionService(DeviceService deviceService)
        {
            _deviceService = deviceService;
        }

        public async Task<FormattedResultModel> ExecuteCommandAsync(
            string deviceId,
            int commandIndex,
            string[] parameterValues,
            string formatSchema)
        {
            // Execute the command
            var result = await _deviceService.ExecuteDeviceCommandAsync(deviceId, commandIndex, parameterValues);

            // Format the result based on the format schema
            return FormatResult(result.Result, formatSchema);
        }

        private FormattedResultModel FormatResult(string rawResult, string formatSchema)
        {
            // For simplicity, we'll just return the raw result with some basic formatting
            // In a real implementation, this would parse the formatSchema (which is an OpenAPI schema)
            // and format the rawResult accordingly
            
            var formatted = new FormattedResultModel
            {
                RawResult = rawResult,
                FormattedResult = rawResult
            };

            // Try to parse as JSON if format schema implies it's a JSON
            if (formatSchema.Contains("object") || formatSchema.Contains("array"))
            {
                try
                {
                    // Try to parse as JSON and pretty-print
                    var jsonElement = JsonSerializer.Deserialize<JsonElement>(rawResult);
                    formatted.FormattedResult = JsonSerializer.Serialize(jsonElement, new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });
                    formatted.IsJson = true;
                }
                catch
                {
                    // If parsing fails, just use the raw result
                    formatted.IsJson = false;
                }
            }

            return formatted;
        }
    }

    public class FormattedResultModel
    {
        public string RawResult { get; set; }
        public string FormattedResult { get; set; }
        public bool IsJson { get; set; }
    }
}