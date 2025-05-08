using System.Collections.Generic;
using System.Threading.Tasks;
using IoTManagement.Domain.Entities;

namespace IoTManagement.Domain.Services
{
    /// <summary>
    /// Servi�o de dom�nio para gerenciamento e execu��o de comandos de dispositivos.
    /// </summary>
    public interface IDeviceCommandService
    {
        /// <summary>
        /// Obt�m um comando espec�fico de um dispositivo.
        /// </summary>
        /// <param name="deviceId">ID do dispositivo</param>
        /// <param name="commandId">ID do comando</param>
        /// <returns>O comando do dispositivo</returns>
        Task<DeviceCommand> GetDeviceCommandAsync(string deviceId, string commandId);

        /// <summary>
        /// Obt�m todos os comandos dispon�veis para um dispositivo.
        /// </summary>
        /// <param name="deviceId">ID do dispositivo</param>
        /// <returns>Lista de comandos do dispositivo</returns>
        Task<List<DeviceCommand>> GetDeviceCommandsAsync(string deviceId);

        /// <summary>
        /// Executa um comando em um dispositivo.
        /// </summary>
        /// <param name="deviceId">ID do dispositivo</param>
        /// <param name="commandId">ID do comando</param>
        /// <param name="parameterValues">Valores dos par�metros para o comando</param>
        /// <returns>Resultado da execu��o do comando</returns>
        Task<string> ExecuteDeviceCommandAsync(string deviceId, string commandId, string[] parameterValues);
    }
}