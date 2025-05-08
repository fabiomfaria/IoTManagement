using System.Threading.Tasks;
using IoTManagement.Domain.Entities;

namespace IoTManagement.Domain.Services
{
    /// <summary>
    /// Serviço responsável pela execução de comandos em dispositivos IoT
    /// </summary>
    public interface IDeviceCommandExecutionService
    {
        /// <summary>
        /// Executa um comando específico em um dispositivo usando os valores de parâmetros fornecidos
        /// </summary>
        /// <param name="device">O dispositivo alvo</param>
        /// <param name="command">O comando a ser executado</param>
        /// <param name="parameterValues">Os valores dos parâmetros para o comando</param>
        /// <returns>O resultado da execução do comando como string</returns>
        Task<string> ExecuteDeviceCommandAsync(Device device, DeviceCommand command, string[] parameterValues);
    }
}