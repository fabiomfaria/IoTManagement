using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using IoTManagement.UI.Blazor.Models;

namespace IoTManagement.UI.Blazor.Services
{
    public class DeviceService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;

        public DeviceService(HttpClient httpClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
        }

        private async Task SetAuthHeader()
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<List<string>> GetAllDeviceIdsAsync()
        {
            await SetAuthHeader();
            return await _httpClient.GetFromJsonAsync<List<string>>("api/devices");
        }

        public async Task<DeviceModel> GetDeviceByIdAsync(string id)
        {
            await SetAuthHeader();
            return await _httpClient.GetFromJsonAsync<DeviceModel>($"api/devices/{id}");
        }

        public async Task<CommandResultModel> ExecuteDeviceCommandAsync(
            string deviceId, 
            int commandIndex, 
            string[] parameterValues)
        {
            await SetAuthHeader();
            
            var request = new ExecuteCommandModel
            {
                ParameterValues = parameterValues
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"api/devices/{deviceId}/commands/{commandIndex}/execute", 
                request);

            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<CommandResultModel>(content, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
            return result;
        }
    }
}