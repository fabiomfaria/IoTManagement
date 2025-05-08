using System;
using System.Threading.Tasks;
using IoTManagement.Domain.Entities;
using IoTManagement.Domain.Exceptions;
using IoTManagement.Domain.Services;
using Microsoft.Extensions.Logging;

namespace IoTManagement.Infrastructure.Services
{
    /// <summary>
    /// Implementação do serviço de execução de comandos em dispositivos IoT
    /// </summary>
    public class DeviceCommandExecutionService : IDeviceCommandExecutionService
    {
        private readonly ITelnetClient _telnetClient;
        private readonly ILogger<DeviceCommandExecutionService> _logger;

        public DeviceCommandExecutionService(ITelnetClient telnetClient, ILogger<DeviceCommandExecutionService> logger)
        {
            _telnetClient = telnetClient ?? throw new ArgumentNullException(nameof(telnetClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public async Task<string> ExecuteDeviceCommandAsync(Device device, DeviceCommand command, string[] parameterValues)
        {
            // Validar parâmetros
            if (device == null)
                throw new ArgumentNullException(nameof(device));

            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (parameterValues == null)
                throw new ArgumentNullException(nameof(parameterValues));

            // Validar que o dispositivo tem uma URL de Telnet definida
            if (string.IsNullOrEmpty(device.TelnetUrl))
                throw new ValidationException("O dispositivo não possui uma URL Telnet configurada");

            // Validar que o número de parâmetros corresponde ao esperado pelo comando
            if (command.Command.Parameters.Count != parameterValues.Length)
                throw new ValidationException($"O comando espera {command.Command.Parameters.Count} parâmetros, mas foram fornecidos {parameterValues.Length}");

            try
            {
                _logger.LogInformation("Executando comando '{CommandName}' no dispositivo '{DeviceId}'", command.Name, device.Identifier);
                
                // Executar o comando via Telnet
                var response = await _telnetClient.ExecuteCommandAsync(
                    device.TelnetUrl,
                    command.Command,
                    parameterValues);

                _logger.LogInformation("Comando executado com sucesso. Resposta: {Response}", response);
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao executar comando '{CommandName}' no dispositivo '{DeviceId}'", command.Name, device.Identifier);
                throw new TelnetCommunicationException($"Erro na comunicação com o dispositivo: {ex.Message}", ex);
            }
        }
    }
}