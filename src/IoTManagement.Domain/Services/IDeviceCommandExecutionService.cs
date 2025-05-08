using System.Threading.Tasks;
using IoTManagement.Domain.Entities;

namespace IoTManagement.Domain.Services
{
    /// <summary>
    /// Servi�o respons�vel pela execu��o de comandos em dispositivos IoT
    /// </summary>
    public interface IDeviceCommandExecutionService
    {
        /// <summary>
        /// Executa um comando espec�fico em um dispositivo usando os valores de par�metros fornecidos
        /// </summary>
        /// <param name="device">O dispositivo alvo</param>
        /// <param name="command">O comando a ser executado</param>
        /// <param name="parameterValues">Os valores dos par�metros para o comando</param>
        /// <returns>O resultado da execu��o do comando como string</returns>
        Task<string> ExecuteDeviceCommandAsync(Device device, DeviceCommand command, string[] parameterValues);
    }
}