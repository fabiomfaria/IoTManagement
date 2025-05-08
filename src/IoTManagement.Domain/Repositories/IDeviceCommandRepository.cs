using System.Collections.Generic;
using System.Threading.Tasks;
using IoTManagement.Domain.Entities;

namespace IoTManagement.Domain.Repositories
{
    public interface IDeviceCommandRepository
    {
        /// <summary>
        /// Obtém um comando específico de um dispositivo
        /// </summary>
        Task<DeviceCommand> GetDeviceCommandAsync(string deviceId, string commandId);

        /// <summary>
        /// Obtém todos os comandos de um dispositivo
        /// </summary>
        Task<List<DeviceCommand>> GetDeviceCommandsAsync(string deviceId);
    }
}
