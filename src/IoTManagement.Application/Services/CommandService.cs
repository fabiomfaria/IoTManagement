using IoTManagement.Application.DTOs;
using IoTManagement.Application.Interfaces;
using IoTManagement.Domain.Entities;
using IoTManagement.Domain.Exceptions;
using IoTManagement.Domain.Repositories;
using IoTManagement.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IoTManagement.Application.Services
{
    public class CommandService : ICommandService
    {
        private readonly IDeviceRepository _deviceRepository;
        private readonly ITelnetClient _telnetClient;

        public CommandService(IDeviceRepository deviceRepository, ITelnetClient telnetClient)
        {
            _deviceRepository = deviceRepository;
            _telnetClient = telnetClient;
        }

        public async Task<IEnumerable<DeviceCommandDto>> GetDeviceCommandsAsync(string deviceId)
        {
            var device = await _deviceRepository.GetByIdAsync(deviceId);
            if (device == null)
            {
                throw new ValidationException($"Device with ID {deviceId} not found.");
            }

            return device.Commands.Select(MapCommandToDto);
        }

        public async Task<DeviceCommandDto> GetDeviceCommandAsync(string deviceId, string commandId)
        {
            var device = await _deviceRepository.GetByIdAsync(deviceId);
            if (device == null)
            {
                throw new ValidationException($"Device with ID {deviceId} not found.");
            }

            var command = device.Commands.FirstOrDefault(c => c.Id == commandId);
            if (command == null)
            {
                throw new ValidationException($"Command with ID {commandId} not found for device {deviceId}.");
            }

            return MapCommandToDto(command);
        }

        public async Task<DeviceCommandExecutionResponseDto> ExecuteCommandAsync(DeviceCommandExecutionRequestDto request)
        {
            // Validate request
            if (string.IsNullOrWhiteSpace(request.DeviceId))
            {
                throw new ValidationException("DeviceId is required.");
            }

            if (string.IsNullOrWhiteSpace(request.CommandId))
            {
                throw new ValidationException("CommandId is required.");
            }

            // Get device and command
            var device = await _deviceRepository.GetByIdAsync(request.DeviceId);
            if (device == null)
            {
                throw new ValidationException($"Device with ID {request.DeviceId} not found.");
            }

            var command = device.Commands.FirstOrDefault(c => c.Id == request.CommandId);
            if (command == null)
            {
                throw new ValidationException($"Command with ID {request.CommandId} not found for device {request.DeviceId}.");
            }

            // Validate parameters
            foreach (var param in command.Parameters)
            {
                if (param.Required && (!request.Parameters.ContainsKey(param.Name) || string.IsNullOrWhiteSpace(request.Parameters[param.Name])))
                {
                    throw new ValidationException($"Required parameter '{param.Name}' is missing or empty.");
                }
            }

            // Build command string
            var commandText = command.CommandText;
            var parameterValues = new List<string>();

            foreach (var param in command.Parameters)
            {
                string value = param.DefaultValue ?? string.Empty;
                if (request.Parameters.ContainsKey(param.Name) && !string.IsNullOrWhiteSpace(request.Parameters[param.Name]))
                {
                    value = request.Parameters[param.Name];
                }

                parameterValues.Add(value);
            }

            var telnetCommand = $"{commandText} {string.Join(" ", parameterValues)}".Trim() + "\r";

            try
            {
                // Connect to device via telnet
                await _telnetClient.ConnectAsync(device.IpAddress, device.Port);

                // Send command and get response
                var rawResponse = await _telnetClient.SendCommandAsync(telnetCommand);

                // Format response according to response format
                var formattedResponse = FormatResponse(rawResponse, command.ResponseFormat);

                return new DeviceCommandExecutionResponseDto
                {
                    DeviceId = request.DeviceId,
                    CommandId = request.CommandId,
                    CommandName = command.Name,
                    RawResponse = rawResponse,
                    FormattedResponse = formattedResponse,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                return new DeviceCommandExecutionResponseDto
                {
                    DeviceId = request.DeviceId,
                    CommandId = request.CommandId,
                    CommandName = command.Name,
                    Success = false,
                    ErrorMessage = $"Error executing command: {ex.Message}"
                };
            }
            finally
            {
                await _telnetClient.DisconnectAsync();
            }
        }

        private DeviceCommandDto MapCommandToDto(DeviceCommand command)
        {
            return new DeviceCommandDto
            {
                Id = command.Id,
                Name = command.Name,
                Description = command.Description,
                ResultDescription = command.ResultDescription,
                CommandText = command.CommandText,
                ResponseFormat = command.ResponseFormat,
                Parameters = command.Parameters.Select(p => new DeviceCommandParameterDto
                {
                    Name = p.Name,
                    Description = p.Description,
                    Type = p.Type,
                    Required = p.Required,
                    DefaultValue = p.DefaultValue
                }).ToList()
            };
        }

        private object FormatResponse(string rawResponse, string responseFormat)
        {
            // Remove carriage return at the end if present
            rawResponse = rawResponse.TrimEnd('\r', '\n');

            // If response format is JSON, attempt to parse the response as JSON
            if (responseFormat.Equals("json", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    return JsonSerializer.Deserialize<object>(rawResponse);
                }
                catch
                {
                    // If parsing fails, return raw response
                    return rawResponse;
                }
            }

            // If response format is CSV, parse into an array of dictionaries
            if (responseFormat.Equals("csv", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var lines = rawResponse.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    var headers = lines[0].Split(',');
                    var result = new List<Dictionary<string, string>>();

                    for (int i = 1; i < lines.Length; i++)
                    {
                        var values = lines[i].Split(',');
                        var row = new Dictionary<string, string>();

                        for (int j = 0; j < Math.Min(headers.Length, values.Length); j++)
                        {
                            row[headers[j]] = values[j];
                        }

                        result.Add(row);
                    }

                    return result;
                }
                catch
                {
                    // If parsing fails, return raw response
                    return rawResponse;
                }
            }

            // For regex format, extract groups
            if (responseFormat.StartsWith("regex:"))
            {
                try
                {
                    var regexPattern = responseFormat.Substring(6); // Remove "regex:" prefix
                    var match = Regex.Match(rawResponse, regexPattern);

                    if (match.Success)
                    {
                        var result = new Dictionary<string, string>();
                        for (int i = 1; i < match.Groups.Count; i++)
                        {
                            var group = match.Groups[i];
                            if (group.Name != i.ToString()) // Named group
                            {
                                result[group.Name] = group.Value;
                            }
                        }
                        return result;
                    }
                }
                catch
                {
                    // If parsing fails, return raw response
                    return rawResponse;
                }
            }

            // Default: return raw response
            return rawResponse;
        }
    }
}