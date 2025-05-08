using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using IoTManagement.Domain.Entities;
using IoTManagement.Domain.Repositories;
using Microsoft.Extensions.Configuration;

namespace IoTManagement.Infrastructure.Repositories
{
    /// <summary>
    /// Implementação do repositório de dispositivos que se comunica com a API CIoTD
    /// </summary>
    /// </summary>marymPorta
    public class CIoTDDeviceRepository : IDeviceRepository
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseApiUrl;
        private readonly string _username;
        private readonly string _password;

        public CIoTDDeviceRepository(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            
            // Obter configurações
            _baseApiUrl = configuration["CIoTDApi:BaseUrl"] ?? throw new ArgumentNullException("CIoTDApi:BaseUrl");
            _username = configuration["CIoTDApi:Username"] ?? throw new ArgumentNullException("CIoTDApi:Username");
            _password = configuration["CIoTDApi:Password"] ?? throw new ArgumentNullException("CIoTDApi:Password");
            
            // Configurar autenticação básica
            var authValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_username}:{_password}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
        }

        /// <summary>
        /// Obtém a lista de identificadores de todos os dispositivos
        /// </summary>
        public async Task<List<string>> GetAllDeviceIdsAsync()
        {
            var response = await _httpClient.GetAsync($"{_baseApiUrl}/device");
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<string>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        /// <summary>
        /// Obtém um dispositivo pelo seu identificador
        /// </summary>
        public async Task<Device> GetDeviceByIdAsync(string id)
        {
            var response = await _httpClient.GetAsync($"{_baseApiUrl}/device/{id}");
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;
                
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Device>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        /// <summary>
        /// Adiciona um novo dispositivo
        /// </summary>
        public async Task<string> AddDeviceAsync(Device device)
        {
            var content = new StringContent(
                JsonSerializer.Serialize(device),
                Encoding.UTF8,
                "application/json");
                
            var response = await _httpClient.PostAsync($"{_baseApiUrl}/device", content);
            response.EnsureSuccessStatusCode();
            
            // Extrair ID do dispositivo da URL de resposta
            var locationHeader = response.Headers.Location;
            return locationHeader.Segments[^1];
        }

        /// <summary>
        /// Atualiza um dispositivo existente
        /// </summary>
        public async Task<bool> UpdateDeviceAsync(Device device)
        {
            var content = new StringContent(
                JsonSerializer.Serialize(device),
                Encoding.UTF8,
                "application/json");
                
            var response = await _httpClient.PutAsync($"{_baseApiUrl}/device/{device.Identifier}", content);
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return false;
                
            response.EnsureSuccessStatusCode();
            return true;
        }

        /// <summary>
        /// Remove um dispositivo
        /// </summary>
        public async Task<bool> DeleteDeviceAsync(string id)
        {
            var response = await _httpClient.DeleteAsync($"{_baseApiUrl}/device/{id}");
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound ||
                response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                return false;
                
            response.EnsureSuccessStatusCode();
            return true;
        }
    }
}