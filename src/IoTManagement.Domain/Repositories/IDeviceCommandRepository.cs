using System.Collections.Generic;
using System.Threading.Tasks;
using IoTManagement.Domain.Entities;

namespace IoTManagement.Domain.Repositories
{
    public interface IDeviceCommandRepository
    {
        /// <summary>
        /// Obt�m um comando espec�fico de um dispositivo
        /// </summary>
        Task<DeviceCommand> GetDeviceCommandAsync(string deviceId, string commandId);

        /// <summary>
        /// Obt�m todos os comandos de um dispositivo
        /// </summary>
        Task<List<DeviceCommand>> GetDeviceCommandsAsync(string deviceId);
    }
}
