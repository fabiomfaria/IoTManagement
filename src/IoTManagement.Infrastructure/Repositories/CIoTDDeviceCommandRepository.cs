using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using IoTManagement.Domain.Entities;
using IoTManagement.Domain.Exceptions;
using IoTManagement.Domain.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IoTManagement.Infrastructure.Repositories
{
    /// <summary>
    /// Implementação do repositório de comandos de dispositivos que se comunica com a API CIoTD
    /// </summary>
    public class CIoTDDeviceCommandRepository : IDeviceCommandRepository
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseApiUrl;
        private readonly ILogger<CIoTDDeviceCommandRepository> _logger;

        public CIoTDDeviceCommandRepository(HttpClient httpClient, IConfiguration configuration, ILogger<CIoTDDeviceCommandRepository> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Obter configurações
            _baseApiUrl = configuration["CIoTDApi:BaseUrl"] ?? throw new ArgumentNullException("CIoTDApi:BaseUrl");
            
            // As configurações de autenticação já devem estar definidas no HttpClient
        }

        /// <summary>
        /// Obtém um comando específico de um dispositivo
        /// </summary>
        public async Task<DeviceCommand> GetDeviceCommandAsync(string deviceId, string commandId)
        {
            try
            {
                // Primeiro obtém o dispositivo completo
                var response = await _httpClient.GetAsync($"{_baseApiUrl}/device/{deviceId}");
                
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;
                    
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var device = JsonSerializer.Deserialize<Device>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                // Encontra o comando específico
                var command = device?.Commands?.FirstOrDefault(c => c.Id == commandId);
                
                return command;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro ao obter comando {CommandId} do dispositivo {DeviceId}", commandId, deviceId);
                throw new Exception($"Erro ao comunicar com a API CIoTD: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtém todos os comandos de um dispositivo
        /// </summary>
        public async Task<List<DeviceCommand>> GetDeviceCommandsAsync(string deviceId)
        {
            try
            {
                // Obtém o dispositivo completo
                var response = await _httpClient.GetAsync($"{_baseApiUrl}/device/{deviceId}");
                
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return new List<DeviceCommand>();
                    
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var device = JsonSerializer.Deserialize<Device>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                return device?.Commands ?? new List<DeviceCommand>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro ao obter comandos do dispositivo {DeviceId}", deviceId);
                throw new Exception($"Erro ao comunicar com a API CIoTD: {ex.Message}", ex);
            }
        }
    }
}