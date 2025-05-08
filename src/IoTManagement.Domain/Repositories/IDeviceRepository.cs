using System.Collections.Generic;
using System.Threading.Tasks;
using IoTManagement.Domain.Entities;

namespace IoTManagement.Domain.Repositories
{
    /// <summary>
    /// Interface para repositório de dispositivos.
    /// </summary>
    public interface IDeviceRepository
    {
        /// <summary>
        /// Obtém a lista de identificadores de todos os dispositivos
        /// </summary>
        Task<List<string>> GetAllDeviceIdsAsync();

        /// <summary>
        /// Obtém um dispositivo pelo seu identificador
        /// </summary>
        Task<Device> GetDeviceByIdAsync(string id);

        /// <summary>
        /// Adiciona um novo dispositivo
        /// </summary>
        Task<string> AddDeviceAsync(Device device);

        /// <summary>
        /// Atualiza um dispositivo existente
        /// </summary>
        Task<bool> UpdateDeviceAsync(Device device);

        /// <summary>
        /// Remove um dispositivo
        /// </summary>
        Task<bool> DeleteDeviceAsync(string id);
    }
}