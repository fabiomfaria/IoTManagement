using IoTManagement.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IoTManagement.API.Interfaces
{
    /// <summary>
    /// Interface para o serviço de comunicação com a API CIoTD
    /// </summary>
    public interface ICIoTDApiService
    {
        /// <summary>
        /// Obtém todos os dispositivos disponíveis
        /// </summary>
        /// <returns>Lista de identificadores de dispositivos</returns>
        Task<List<string>> GetAllDeviceIdsAsync();

        /// <summary>
        /// Obtém detalhes de um dispositivo pelo ID
        /// </summary>
        /// <param name="deviceId">ID do dispositivo</param>
        /// <returns>Detalhes do dispositivo</returns>
        Task<Device> GetDeviceByIdAsync(string deviceId);

        /// <summary>
        /// Executa um comando em um dispositivo
        /// </summary>
        /// <param name="deviceId">ID do dispositivo</param>
        /// <param name="commandId">ID do comando</param>
        /// <param name="parameters">Parâmetros do comando</param>
        /// <returns>Resultado da execução do comando</returns>
        Task<string> ExecuteDeviceCommandAsync(string deviceId, string commandId, Dictionary<string, string> parameters);

        /// <summary>
        /// Verifica se um dispositivo está online
        /// </summary>
        /// <param name="deviceId">ID do dispositivo</param>
        /// <returns>True se o dispositivo estiver online, false caso contrário</returns>
        Task<bool> IsDeviceOnlineAsync(string deviceId);

        /// <summary>
        /// Autentica na API CIoTD
        /// </summary>
        /// <returns>Token de autenticação</returns>
        Task<string> AuthenticateAsync();
    }
}