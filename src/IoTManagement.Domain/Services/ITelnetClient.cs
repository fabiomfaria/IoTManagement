using System.Threading.Tasks;
using IoTManagement.Domain.Entities; // This refers to the nested Command and Parameter types within Device.cs

namespace IoTManagement.Domain.Services
{
    /// <summary>
    /// Interface para cliente de comunicação Telnet com dispositivos.
    /// The implementation of this interface (e.g., in Infrastructure layer)
    /// will handle the actual Telnet socket communication and command formatting based on
    /// the provided 'Command' structure and parameter values, adhering to requirement vii.ii.
    /// </summary>
    public interface ITelnetClient
    {
        /// <summary>
        /// Executa um comando em um dispositivo via telnet.
        /// The implementation will format the command string using command.CommandText
        /// and append parameterValues, separated by spaces, and terminate with '\r'.
        /// </summary>
        /// <param name="deviceUrl">URL (host:port) do dispositivo.</param>
        /// <param name="command">A estrutura do comando do dispositivo (de Device.CommandDescription.Command).</param>
        /// <param name="parameterValues">Valores dos parâmetros para o comando.</param>
        /// <returns>Resposta crua (string) do dispositivo, até o terminador '\r'.</returns>
        Task<string> ExecuteCommandAsync(string deviceUrl, Command command, string[] parameterValues);
    }
}