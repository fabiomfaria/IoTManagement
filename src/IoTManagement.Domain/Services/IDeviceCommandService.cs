using System.Collections.Generic;
using System.Threading.Tasks;
using IoTManagement.Domain.Entities;

namespace IoTManagement.Domain.Services
{
    /// <summary>
    /// Serviço de domínio para gerenciamento e execução de comandos de dispositivos.
    /// </summary>
    public interface IDeviceCommandService
    {
        /// <summary>
        /// Obtém um comando específico de um dispositivo.
        /// </summary>
        /// <param name="deviceId">ID do dispositivo</param>
        /// <param name="commandId">ID do comando</param>
        /// <returns>O comando do dispositivo</returns>
        Task<DeviceCommand> GetDeviceCommandAsync(string deviceId, string commandId);

        /// <summary>
        /// Obtém todos os comandos disponíveis para um dispositivo.
        /// </summary>
        /// <param name="deviceId">ID do dispositivo</param>
        /// <returns>Lista de comandos do dispositivo</returns>
        Task<List<DeviceCommand>> GetDeviceCommandsAsync(string deviceId);

        /// <summary>
        /// Executa um comando em um dispositivo.
        /// </summary>
        /// <param name="deviceId">ID do dispositivo</param>
        /// <param name="commandId">ID do comando</param>
        /// <param name="parameterValues">Valores dos parâmetros para o comando</param>
        /// <returns>Resultado da execução do comando</returns>
        Task<string> ExecuteDeviceCommandAsync(string deviceId, string commandId, string[] parameterValues);
    }
}