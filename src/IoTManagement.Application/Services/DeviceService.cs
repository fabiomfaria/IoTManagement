using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IoTManagement.Domain.Entities;
using IoTManagement.Domain.Repositories;
using IoTManagement.Domain.Services;

namespace IoTManagement.Application.Services
{
    /// <summary>
    /// Serviço para gerenciamento de dispositivos IoT
    /// </summary>
    public class DeviceService : IDeviceService
    {
        private readonly IDeviceRepository _deviceRepository;
        private readonly ITelnetClient _telnetClient;

        public DeviceService(IDeviceRepository deviceRepository, ITelnetClient telnetClient)
        {
            _deviceRepository = deviceRepository ?? throw new ArgumentNullException(nameof(deviceRepository));
            _telnetClient = telnetClient ?? throw new ArgumentNullException(nameof(telnetClient));
        }

        /// <summary>
        /// Obtém a lista de todos os dispositivos
        /// </summary>
        public async Task<List<string>> GetAllDeviceIdsAsync()
        {
            return await _deviceRepository.GetAllDeviceIdsAsync();
        }

        /// <summary>
        /// Obtém os detalhes de um dispositivo
        /// </summary>
        public async Task<Device> GetDeviceDetailsAsync(string id)
        {
            return await _deviceRepository.GetDeviceByIdAsync(id);
        }

        /// <summary>
        /// Executa um comando em um dispositivo
        /// </summary>
        public async Task<string> ExecuteDeviceCommandAsync(string deviceId, int commandIndex, string[] parameterValues)
        {
            // Obter o dispositivo
            var device = await _deviceRepository.GetDeviceByIdAsync(deviceId);
            if (device == null)
                throw new KeyNotFoundException($"Dispositivo com ID {deviceId} não encontrado");

            // Verificar se o índice do comando é válido
            if (commandIndex < 0 || commandIndex >= device.Commands.Count)
                throw new ArgumentOutOfRangeException(nameof(commandIndex), "Índice de comando inválido");

            // Obter o comando
            var commandDesc = device.Commands[commandIndex];
            
            // Verificar se o número de parâmetros é correto
            if (parameterValues.Length != commandDesc.Command.Parameters.Count)
                throw new ArgumentException("Número de parâmetros incorreto");

            // Executar o comando via telnet
            return await _telnetClient.ExecuteCommandAsync(device.Url, commandDesc.Command, parameterValues);
        }
    }

    /// <summary>
    /// Interface para o serviço de dispositivos
    /// </summary>
    public interface IDeviceService
    {
        Task<List<string>> GetAllDeviceIdsAsync();
        Task<Device> GetDeviceDetailsAsync(string id);
        Task<string> ExecuteDeviceCommandAsync(string deviceId, int commandIndex, string[] parameterValues);
    }
}