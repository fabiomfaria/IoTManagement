using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using IoTManagement.Domain.Entities;
using IoTManagement.Domain.Services;

namespace IoTManagement.Infrastructure.Services
{
    /// <summary>
    /// Implementação de cliente Telnet para comunicação com dispositivos IoT
    /// </summary>
    public class TelnetClient : ITelnetClient
    {
        /// <summary>
        /// Executa um comando em um dispositivo via telnet
        /// </summary>
        public async Task<string> ExecuteCommandAsync(string deviceUrl, Command command, string[] parameterValues)
        {
            // Validar parâmetros
            if (string.IsNullOrEmpty(deviceUrl))
                throw new ArgumentNullException(nameof(deviceUrl));
            
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            
            if (parameterValues == null)
                throw new ArgumentNullException(nameof(parameterValues));

            // Extrair host e porta da URL do dispositivo
            var uri = new Uri(deviceUrl);
            var host = uri.Host;
            var port = uri.Port > 0 ? uri.Port : 23; // Porta padrão para telnet

            using (var client = new TcpClient())
            {
                try
                {
                    // Conectar ao dispositivo
                    await client.ConnectAsync(host, port);
                    
                    using (var stream = client.GetStream())
                    using (var reader = new StreamReader(stream))
                    using (var writer = new StreamWriter(stream) { AutoFlush = true })
                    {
                        // Montar o comando com os parâmetros
                        var fullCommand = new StringBuilder(command.CommandText);
                        
                        foreach (var param in parameterValues)
                        {
                            fullCommand.Append(' ').Append(param);
                        }
                        
                        // Adicionar terminador de linha
                        fullCommand.Append('\r');
                        
                        // Enviar o comando
                        await writer.WriteAsync(fullCommand.ToString());
                        
                        // Receber a resposta até encontrar um terminador de linha
                        var response = new StringBuilder();
                        int nextChar;
                        while ((nextChar = await reader.ReadAsync()) != -1)
                        {
                            char ch = (char)nextChar;
                            response.Append(ch);
                            
                            if (ch == '\r')
                                break;
                        }
                        
                        return response.ToString().TrimEnd('\r');
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Erro ao se comunicar com o dispositivo: {ex.Message}", ex);
                }
            }
        }
    }
}