using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IoTManagement.Domain.Entities;
using IoTManagement.Domain.Exceptions;
using IoTManagement.Domain.Repositories;
using IoTManagement.Domain.Services;
using Microsoft.Extensions.Logging;

namespace IoTManagement.Infrastructure.Services
{
    /// <summary>
    /// Implementação do serviço de gerenciamento de comandos de dispositivos
    /// </summary>
    public class DeviceCommandService : IDeviceCommandService
    {
        private readonly IDeviceCommandRepository _deviceCommandRepository;
        private readonly IDeviceRepository _deviceRepository;
        private readonly IDeviceCommandExecutionService _executionService;
        private readonly ILogger<DeviceCommandService> _logger;

        public DeviceCommandService(
            IDeviceCommandRepository deviceCommandRepository,
            IDeviceRepository deviceRepository,
            IDeviceCommandExecutionService executionService,
            ILogger<DeviceCommandService> logger)
        {
            _deviceCommandRepository = deviceCommandRepository ?? throw new ArgumentNullException(nameof(deviceCommandRepository));
            _deviceRepository = deviceRepository ?? throw new ArgumentNullException(nameof(deviceRepository));
            _executionService = executionService ?? throw new ArgumentNullException(nameof(executionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<DeviceCommand> GetDeviceCommandAsync(string deviceId, string commandId)
        {
            // Validar parâmetros
            if (string.IsNullOrEmpty(deviceId))
                throw new ArgumentNullException(nameof(deviceId));
                
            if (string.IsNullOrEmpty(commandId))
                throw new ArgumentNullException(nameof(commandId));

            try
            {
                var command = await _deviceCommandRepository.GetDeviceCommandAsync(deviceId, commandId);
                
                if (command == null)
                    throw new DeviceCommandNotFoundException($"O comando '{commandId}' não foi encontrado para o dispositivo '{deviceId}'");
                    
                return command;
            }
            catch (DeviceCommandNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter comando {CommandId} do dispositivo {DeviceId}", commandId, deviceId);
                throw new Exception($"Erro ao obter comando: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<List<DeviceCommand>> GetDeviceCommandsAsync(string deviceId)
        {
            // Validar parâmetros
            if (string.IsNullOrEmpty(deviceId))
                throw new ArgumentNullException(nameof(deviceId));

            try
            {
                return await _deviceCommandRepository.GetDeviceCommandsAsync(deviceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter comandos do dispositivo {DeviceId}", deviceId);
                throw new Exception($"Erro ao obter comandos: {ex.Message}", ex);
            }
        }

        /// <inheritdoc/>
        public async Task<string> ExecuteDeviceCommandAsync(string deviceId, string commandId, string[] parameterValues)
        {
            // Validar parâmetros
            if (string.IsNullOrEmpty(deviceId))
                throw new ArgumentNullException(nameof(deviceId));
                
            if (string.IsNullOrEmpty(commandId))
                throw new ArgumentNullException(nameof(commandId));
                
            if (parameterValues == null)
                throw new ArgumentNullException(nameof(parameterValues));

            try
            {
                // Obter o dispositivo
                var device = await _deviceRepository.GetDeviceByIdAsync(deviceId);
                if (device == null)
                    throw new Exception($"Dispositivo '{deviceId}' não encontrado");

                // Obter o comando
                var command = await _deviceCommandRepository.GetDeviceCommandAsync(deviceId, commandId);
                if (command == null)
                    throw new DeviceCommandNotFoundException($"O comando '{commandId}' não foi encontrado para o dispositivo '{deviceId}'");

                // Executar o comando
                return await _executionService.ExecuteDeviceCommandAsync(device, command, parameterValues);
            }
            catch (DeviceCommandNotFoundException)
            {
                throw;
            }
            catch (TelnetCommunicationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao executar comando {CommandId} no dispositivo {DeviceId}", commandId, deviceId);
                throw new Exception($"Erro ao executar comando: {ex.Message}", ex);
            }
        }
    }
}