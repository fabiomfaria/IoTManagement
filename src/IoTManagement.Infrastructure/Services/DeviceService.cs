using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IoTManagement.Application.DTOs;
using IoTManagement.Application.Interfaces;
using IoTManagement.Domain.Entities;
using IoTManagement.Domain.Exceptions;
using IoTManagement.Domain.Repositories;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace IoTManagement.Infrastructure.Services
{
    /// <summary>
    /// Implementação do serviço de gerenciamento de dispositivos
    /// </summary>
    public class DeviceService : IDeviceService
    {
        private readonly IDeviceRepository _deviceRepository;
        private readonly ILogger<DeviceService> _logger;

        public DeviceService(IDeviceRepository deviceRepository, ILogger<DeviceService> logger)
        {
            _deviceRepository = deviceRepository ?? throw new ArgumentNullException(nameof(deviceRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<List<DeviceDto>> GetAllDevicesAsync()
        {
            try
            {
                _logger.LogInformation("Obtendo lista de todos os dispositivos");
                
                // Obter os IDs dos dispositivos
                var deviceIds = await _deviceRepository.GetAllDeviceIdsAsync();
                
                var deviceList = new List<DeviceDto>();
                
                // Para cada ID, obter os detalhes do dispositivo
                foreach (var id in deviceIds)
                {
                    var device = await _deviceRepository.GetDeviceByIdAsync(id);
                    if (device != null)
                    {
                        deviceList.Add(MapDeviceToDto(device));
                    }
                }
                
                return deviceList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter lista de dispositivos");
                throw new Exception("Não foi possível obter a lista de dispositivos", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<DeviceDto> GetDeviceByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentNullException(nameof(id));

            try
            {
                _logger.LogInformation("Obtendo dispositivo com ID: {DeviceId}", id);
                
                var device = await _deviceRepository.GetDeviceByIdAsync(id);
                
                if (device == null)
                    return null;
                    
                return MapDeviceToDto(device);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter dispositivo com ID: {DeviceId}", id);
                throw new Exception($"Não foi possível obter o dispositivo com ID: {id}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<List<DeviceCommandDto>> GetDeviceCommandsAsync(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
                throw new ArgumentNullException(nameof(deviceId));

            try
            {
                _logger.LogInformation("Obtendo comandos do dispositivo com ID: {DeviceId}", deviceId);
                
                var device = await _deviceRepository.GetDeviceByIdAsync(deviceId);
                
                if (device == null)
                    return new List<DeviceCommandDto>();
                
                return device.Commands.Select(c => new DeviceCommandDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Description = c.Description,
                    ResultDescription = c.ResultDescription,
                    ResponseFormat = c.Format,
                    Parameters = c.Command.Parameters.Select(p => new DeviceCommandParameterDto
                    {
                        Name = p.Name,
                        Description = p.Description,
                        Type = p.Type
                    }).ToList()
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter comandos do dispositivo com ID: {DeviceId}", deviceId);
                throw new Exception($"Não foi possível obter os comandos do dispositivo com ID: {deviceId}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<DeviceCommandDto> GetDeviceCommandByIdAsync(string deviceId, string commandId)
        {
            if (string.IsNullOrEmpty(deviceId))
                throw new ArgumentNullException(nameof(deviceId));
                
            if (string.IsNullOrEmpty(commandId))
                throw new ArgumentNullException(nameof(commandId));

            try
            {
                _logger.LogInformation("Obtendo comando {CommandId} do dispositivo {DeviceId}", commandId, deviceId);
                
                var device = await _deviceRepository.GetDeviceByIdAsync(deviceId);
                
                if (device == null)
                    return null;
                
                var command = device.Commands.FirstOrDefault(c => c.Id == commandId);
                
                if (command == null)
                    return null;
                
                return new DeviceCommandDto
                {
                    Id = command.Id,
                    Name = command.Name,
                    Description = command.Description,
                    ResultDescription = command.ResultDescription,
                    ResponseFormat = command.Format,
                    Parameters = command.Command.Parameters.Select(p => new DeviceCommandParameterDto
                    {
                        Name = p.Name,
                        Description = p.Description,
                        Type = p.Type
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter comando {CommandId} do dispositivo {DeviceId}", commandId, deviceId);
                throw new Exception($"Não foi possível obter o comando {commandId} do dispositivo {deviceId}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<DeviceCommandExecutionResponseDto> ExecuteCommandAsync(string deviceId, string commandId, DeviceCommandExecutionRequestDto requestDto)
        {
            if (string.IsNullOrEmpty(deviceId))
                throw new ArgumentNullException(nameof(deviceId));
                
            if (string.IsNullOrEmpty(commandId))
                throw new ArgumentNullException(nameof(commandId));
                
            if (requestDto == null)
                throw new ArgumentNullException(nameof(requestDto));

            try
            {
                _logger.LogInformation("Executando comando {CommandId} no dispositivo {DeviceId}", commandId, deviceId);
                
                // Obter o dispositivo
                var device = await _deviceRepository.GetDeviceByIdAsync(deviceId);
                if (device == null)
                    throw new ValidationException($"Dispositivo com ID {deviceId} não encontrado");
                
                // Obter o comando
                var command = device.Commands.FirstOrDefault(c => c.Id == commandId);
                if (command == null)
                    throw new ValidationException($"Comando com ID {commandId} não encontrado no dispositivo {deviceId}");
                
                // Validar que os parâmetros necessários foram fornecidos
                if (command.Command.Parameters.Count != requestDto.ParameterValues.Length)
                    throw new ValidationException($"O comando {command.Name} espera {command.Command.Parameters.Count} parâmetros, mas foram fornecidos {requestDto.ParameterValues.Length}");
                
                // Montar os parâmetros para execução via Telnet
                var telnetParameters = requestDto.ParameterValues;
                
                // Montar o comando para execução via Telnet
                var telnetCommand = new StringBuilder(command.Command.CommandText);
                foreach (var param in telnetParameters)
                {
                    telnetCommand.Append(" ").Append(param);
                }
                telnetCommand.Append("\r");
                
                // Simular a execução do comando via Telnet
                // Em uma implementação real, aqui chamaria o serviço de telnet
                var result = $"Simulação da execução do comando: {telnetCommand}";
                
                return new DeviceCommandExecutionResponseDto
                {
                    DeviceId = deviceId,
                    CommandId = commandId,
                    CommandName = command.Name,
                    Result = result,
                    ExecutedAt = DateTime.UtcNow
                };
            }
            catch (ValidationException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao executar comando {CommandId} no dispositivo {DeviceId}", commandId, deviceId);
                throw new Exception($"Não foi possível executar o comando: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Mapeia uma entidade Device para um DeviceDto
        /// </summary>
        private DeviceDto MapDeviceToDto(Device device)
        {
            if (device == null)
                return null;
                
            return new DeviceDto
            {
                Id = device.Identifier,
                Name = device.Name,
                Manufacturer = device.Manufacturer,
                Description = device.Description,
                TelnetUrl = device.TelnetUrl,
                CommandCount = device.Commands?.Count ?? 0
            };
        }
    }
}