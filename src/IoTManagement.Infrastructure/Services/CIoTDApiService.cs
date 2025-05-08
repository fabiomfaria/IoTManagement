using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using IoTManagement.API.Interfaces;
using IoTManagement.Domain.Entities;
using IoTManagement.Domain.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IoTManagement.Infrastructure.Services
{
    /// <summary>
    /// Implementação do serviço de comunicação com a API CIoTD
    /// </summary>
    public class CIoTDApiService : ICIoTDApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CIoTDApiService> _logger;
        private string _authToken;
        private DateTime _tokenExpiration = DateTime.MinValue;

        public CIoTDApiService(HttpClient httpClient, IConfiguration configuration, ILogger<CIoTDApiService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Configuração do HttpClient
            _httpClient.BaseAddress = new Uri(_configuration["CIoTDApi:BaseUrl"] ?? throw new InvalidOperationException("CIoTDApi:BaseUrl not configured"));
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Obtém todos os dispositivos disponíveis
        /// </summary>
        public async Task<List<string>> GetAllDeviceIdsAsync()
        {
            try
            {
                await EnsureAuthenticatedAsync();
                
                var response = await _httpClient.GetAsync("devices");
                response.EnsureSuccessStatusCode();
                
                var deviceIds = await response.Content.ReadFromJsonAsync<List<string>>();
                return deviceIds ?? new List<string>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error retrieving device IDs from CIoTD API");
                throw new ExternalServiceException("Error retrieving device IDs", ex);
            }
        }

        /// <summary>
        /// Obtém detalhes de um dispositivo pelo ID
        /// </summary>
        public async Task<Device> GetDeviceByIdAsync(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                throw new ArgumentException("Device ID cannot be null or empty", nameof(deviceId));
            }

            try
            {
                await EnsureAuthenticatedAsync();
                
                var response = await _httpClient.GetAsync($"devices/{deviceId}");
                response.EnsureSuccessStatusCode();
                
                var deviceDto = await response.Content.ReadFromJsonAsync<DeviceDto>();
                if (deviceDto == null)
                {
                    throw new KeyNotFoundException($"Device with ID {deviceId} not found");
                }
                
                // Mapear o DTO para a entidade de domínio
                var device = new Device
                {
                    Identifier = deviceDto.Identifier,
                    Description = deviceDto.Description,
                    Manufacturer = deviceDto.Manufacturer,
                    Url = deviceDto.Url,
                    Commands = new List<Command>()
                };
                
                // Mapear os comandos
                if (deviceDto.Commands != null)
                {
                    foreach (var cmdDto in deviceDto.Commands)
                    {
                        var command = new Command
                        {
                            Id = cmdDto.Operation,
                            Name = cmdDto.Operation,
                            Description = cmdDto.Description,
                            ResultDescription = cmdDto.Result,
                            CommandDetails = new CommandDetails
                            {
                                Command = cmdDto.Command?.CommandText,
                                Parameters = new List<Parameter>()
                            },
                            Format = new ResponseFormat
                            {
                                Description = cmdDto.Format,
                                Format = cmdDto.Format
                            }
                        };
                        
                        // Mapear os parâmetros
                        if (cmdDto.Command?.Parameters != null)
                        {
                            foreach (var paramDto in cmdDto.Command.Parameters)
                            {
                                command.CommandDetails.Parameters.Add(new Parameter
                                {
                                    Name = paramDto.Name,
                                    Description = paramDto.Description,
                                    Type = "string", // Definindo como string por padrão
                                    Required = true  // Definindo como obrigatório por padrão
                                });
                            }
                        }
                        
                        device.Commands.Add(command);
                    }
                }
                
                return device;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error retrieving device {DeviceId} from CIoTD API", deviceId);
                
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new KeyNotFoundException($"Device with ID {deviceId} not found");
                }
                
                throw new ExternalServiceException($"Error retrieving device {deviceId}", ex);
            }
        }

        /// <summary>
        /// Executa um comando em um dispositivo
        /// </summary>
        public async Task<string> ExecuteDeviceCommandAsync(string deviceId, string commandId, Dictionary<string, string> parameters)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                throw new ArgumentException("Device ID cannot be null or empty", nameof(deviceId));
            }
            
            if (string.IsNullOrEmpty(commandId))
            {
                throw new ArgumentException("Command ID cannot be null or empty", nameof(commandId));
            }

            try
            {
                await EnsureAuthenticatedAsync();
                
                // Encontrar o índice do comando baseado no ID
                var device = await GetDeviceByIdAsync(deviceId);
                var commandIndex = device.Commands.FindIndex(c => c.Id == commandId);
                
                if (commandIndex == -1)
                {
                    throw new KeyNotFoundException($"Command with ID {commandId} not found for device {deviceId}");
                }
                
                // Converter os parâmetros do dicionário para um array
                var parameterValues = new List<string>();
                if (parameters != null)
                {
                    foreach (var param in device.Commands[commandIndex].CommandDetails.Parameters)
                    {
                        if (parameters.TryGetValue(param.Name, out var value))
                        {
                            parameterValues.Add(value);
                        }
                        else if (param.Required)
                        {
                            throw new ValidationException($"Required parameter '{param.Name}' is missing");
                        }
                    }
                }
                
                // Criar o objeto para a requisição
                var requestData = new
                {
                    ParameterValues = parameterValues.ToArray()
                };
                
                // Enviar a requisição para executar o comando
                var content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"devices/{deviceId}/commands/{commandIndex}/execute", content);
                response.EnsureSuccessStatusCode();
                
                // Processar a resposta
                var responseData = await response.Content.ReadFromJsonAsync<CommandExecutionResult>();
                return responseData?.Result ?? string.Empty;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error executing command {CommandId} on device {DeviceId}", commandId, deviceId);
                
                if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new KeyNotFoundException($"Device or command not found");
                }
                
                throw new ExternalServiceException($"Error executing command {commandId} on device {deviceId}", ex);
            }
            catch (TelnetCommunicationException ex)
            {
                _logger.LogError(ex, "Telnet communication error executing command {CommandId} on device {DeviceId}", commandId, deviceId);
                throw;
            }
        }

        /// <summary>
        /// Verifica se um dispositivo está online
        /// </summary>
        public async Task<bool> IsDeviceOnlineAsync(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                throw new ArgumentException("Device ID cannot be null or empty", nameof(deviceId));
            }

            try
            {
                await EnsureAuthenticatedAsync();
                
                var response = await _httpClient.GetAsync($"devices/{deviceId}/status");
                
                // Se a resposta for bem-sucedida, consideramos o dispositivo como online
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error checking online status for device {DeviceId}", deviceId);
                return false; // Assumimos que o dispositivo está offline em caso de erro
            }
        }

        /// <summary>
        /// Autentica na API CIoTD
        /// </summary>
        public async Task<string> AuthenticateAsync()
        {
            try
            {
                // Criar objeto de dados para autenticação
                var authData = new
                {
                    username = _configuration["CIoTDApi:Username"],
                    password = _configuration["CIoTDApi:Password"],
                    grant_type = "password"
                };
                
                // Enviar requisição de autenticação
                var content = new StringContent(JsonSerializer.Serialize(authData), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync("auth/token", content);
                response.EnsureSuccessStatusCode();
                
                // Processar resposta
                var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
                if (tokenResponse == null)
                {
                    throw new UnauthorizedException("Failed to obtain authentication token");
                }
                
                _authToken = tokenResponse.AccessToken;
                _tokenExpiration = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn);
                
                // Atualizar o cabeçalho de autenticação
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
                
                return _authToken;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error authenticating with CIoTD API");
                throw new UnauthorizedException("Failed to authenticate with CIoTD API", ex);
            }
        }

        /// <summary>
        /// Garante que a autenticação está válida antes de fazer chamadas à API
        /// </summary>
        private async Task EnsureAuthenticatedAsync()
        {
            // Se o token estiver expirando em menos de 5 minutos ou já expirou
            if (string.IsNullOrEmpty(_authToken) || _tokenExpiration <= DateTime.UtcNow.AddMinutes(5))
            {
                await AuthenticateAsync();
            }
        }
    }

    /// <summary>
    /// Modelo para resposta de autenticação
    /// </summary>
    internal class TokenResponse
    {
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public int ExpiresIn { get; set; }
    }

    /// <summary>
    /// Modelo para resposta de execução de comando
    /// </summary>
    internal class CommandExecutionResult
    {
        public string Result { get; set; }
    }

    /// <summary>
    /// Modelo para DTO de dispositivo recebido da API CIoTD
    /// </summary>
    internal class DeviceDto
    {
        public string Identifier { get; set; }
        public string Description { get; set; }
        public string Manufacturer { get; set; }
        public string Url { get; set; }
        public List<CommandDescriptionDto> Commands { get; set; }
    }

    /// <summary>
    /// Modelo para DTO de comando recebido da API CIoTD
    /// </summary>
    internal class CommandDescriptionDto
    {
        public string Operation { get; set; }
        public string Description { get; set; }
        public CommandDto Command { get; set; }
        public string Result { get; set; }
        public string Format { get; set; }
    }

    /// <summary>
    /// Modelo para DTO de detalhes de comando recebido da API CIoTD
    /// </summary>
    internal class CommandDto
    {
        public string CommandText { get; set; }
        public List<ParameterDto> Parameters { get; set; }
    }

    /// <summary>
    /// Modelo para DTO de parâmetro recebido da API CIoTD
    /// </summary>
    internal class ParameterDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }
}