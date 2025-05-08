using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Community IoT Device (CIoTD) - MOCK",
        Version = "v1.0.0",
        Description = "Mock API for CIoTD platform."
    });
});

// Simple service to manage mock data
builder.Services.AddSingleton<MockDataService>(sp =>
    new MockDataService(sp.GetRequiredService<IConfiguration>()["MockDataFilePath"] ?? "mock-data.json"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Mock CIoTD API V1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at apps root
    });
}

app.UseHttpsRedirection();

// --- API Endpoints ---

// GET /device
// Retorna uma lista contendo os identificadores dos dispositivos cadastrados na plataforma
app.MapGet("/device", async (MockDataService dataService) =>
{
    var devices = await dataService.GetDevicesAsync();
    return Results.Ok(devices.Select(d => d.Identifier).ToList());
})
.WithName("GetDeviceIdentifiers")
.WithTags("Devices")
.Produces<List<string>>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status401Unauthorized); // Placeholder, mock doesn't enforce auth

// POST /device
// Cadastra um novo dispositivo na plataforma
app.MapPost("/device", async (MockDevice newDevice, MockDataService dataService, HttpRequest request) =>
{
    if (string.IsNullOrWhiteSpace(newDevice.Identifier))
    {
        newDevice.Identifier = Guid.NewGuid().ToString(); // Generate ID if not provided
    }
    var device = await dataService.AddDeviceAsync(newDevice);
    var scheme = request.Scheme;
    var host = request.Host;
    var location = $"{scheme}://{host}/device/{device.Identifier}";
    return Results.Created(location, device);
})
.WithName("CreateDevice")
.WithTags("Devices")
.Produces<MockDevice>(StatusCodes.Status201Created)
.Produces(StatusCodes.Status401Unauthorized);

// GET /device/{id}
// Retorna os detalhes de um dispositivo
app.MapGet("/device/{id}", async (string id, MockDataService dataService) =>
{
    var device = await dataService.GetDeviceByIdAsync(id);
    return device != null ? Results.Ok(device) : Results.NotFound($"Device with id {id} not found.");
})
.WithName("GetDeviceById")
.WithTags("Devices")
.Produces<MockDevice>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status404NotFound);

// PUT /device/{id}
// Atualiza os dados de um dispositivo
app.MapPut("/device/{id}", async (string id, MockDevice updatedDevice, MockDataService dataService) =>
{
    if (id != updatedDevice.Identifier && !string.IsNullOrWhiteSpace(updatedDevice.Identifier))
    {
        return Results.BadRequest("Identifier in URL and body do not match or body identifier is empty.");
    }
    updatedDevice.Identifier = id; // Ensure identifier from URL is used
    var device = await dataService.UpdateDeviceAsync(updatedDevice);
    return device != null ? Results.Ok(device) : Results.NotFound($"Device with id {id} not found.");
})
.WithName("UpdateDevice")
.WithTags("Devices")
.Produces<MockDevice>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status401Unauthorized)
.Produces(StatusCodes.Status404NotFound);

// DELETE /device/{id}
// Remove os detalhes de um dispositivo
app.MapDelete("/device/{id}", async (string id, MockDataService dataService) =>
{
    var device = await dataService.DeleteDeviceAsync(id);
    return device != null ? Results.Ok(device) : Results.NotFound($"Device with id {id} not found.");
})
.WithName("DeleteDevice")
.WithTags("Devices")
.Produces<MockDevice>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status401Unauthorized)
.Produces(StatusCodes.Status404NotFound);


app.Run();

// --- Models (based on API.txt, simplified for mock) ---
public record MockParameter(string Name, string Description);
public record MockCommand(string Command, List<MockParameter>? Parameters);
public record MockCommandDescription(string Operation, string Description, MockCommand Command, string Result, string Format);
public record MockDevice(
    string Identifier,
    string Description,
    string Manufacturer,
    string Url,
    List<MockCommandDescription>? Commands
);

// --- Data Service ---
public class MockDataService
{
    private readonly string _filePath;
    private List<MockDevice> _devices = new();
    private readonly object _lock = new(); // Simple lock for file access

    public MockDataService(string filePath)
    {
        _filePath = filePath;
        LoadDataAsync().Wait(); // Load data on startup
    }

    private async Task LoadDataAsync()
    {
        lock (_lock)
        {
            if (!File.Exists(_filePath))
            {
                // Create a default file if it doesn't exist
                _devices = GetDefaultDevices();
                File.WriteAllText(_filePath, JsonSerializer.Serialize(new { devices = _devices }, new JsonSerializerOptions { WriteIndented = true }));
                return;
            }
            var json = File.ReadAllText(_filePath);
            var data = JsonSerializer.Deserialize<DeviceDataWrapper>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            _devices = data?.Devices ?? GetDefaultDevices();
        }
        await Task.CompletedTask;
    }

    private async Task SaveDataAsync()
    {
        lock (_lock)
        {
            var dataWrapper = new DeviceDataWrapper { Devices = _devices };
            File.WriteAllText(_filePath, JsonSerializer.Serialize(dataWrapper, new JsonSerializerOptions { WriteIndented = true }));
        }
        await Task.CompletedTask;
    }

    private List<MockDevice> GetDefaultDevices()
    {
        return new List<MockDevice>
        {
            new MockDevice(
                "sensor-001",
                "Temperature and Humidity sensor for living room.",
                "HomeBrew Devices",
                "telnet://192.168.1.101:23",
                new List<MockCommandDescription>
                {
                    new MockCommandDescription(
                        "getTemperature",
                        "Reads the current temperature.",
                        new MockCommand("TEMP_READ", new List<MockParameter>()),
                        "Current temperature in Celsius.",
                        "{\"type\": \"object\", \"properties\": {\"temperature\": {\"type\": \"number\", \"format\": \"float\"}}}"
                    ),
                    new MockCommandDescription(
                        "getHumidity",
                        "Reads the current humidity.",
                        new MockCommand("HUM_READ", new List<MockParameter>()),
                        "Current relative humidity in %.",
                        "{\"type\": \"object\", \"properties\": {\"humidity\": {\"type\": \"number\", \"format\": \"float\"}}}"
                    )
                }
            ),
            new MockDevice(
                "actuator-light-002",
                "Smart light switch for the office.",
                "BrightFuture Inc.",
                "telnet://192.168.1.102:23",
                new List<MockCommandDescription>
                {
                    new MockCommandDescription(
                        "turnOn",
                        "Turns the light on.",
                        new MockCommand("LIGHT_ON", new List<MockParameter>()),
                        "Returns status 'ON'.",
                        "{\"type\": \"object\", \"properties\": {\"status\": {\"type\": \"string\", \"enum\": [\"ON\", \"OFF\"]}}}"
                    ),
                    new MockCommandDescription(
                        "turnOff",
                        "Turns the light off.",
                        new MockCommand("LIGHT_OFF", new List<MockParameter>()),
                        "Returns status 'OFF'.",
                        "{\"type\": \"object\", \"properties\": {\"status\": {\"type\": \"string\", \"enum\": [\"ON\", \"OFF\"]}}}"
                    ),
                    new MockCommandDescription(
                        "setBrightness",
                        "Sets the light brightness.",
                        new MockCommand("LIGHT_BRIGHT", new List<MockParameter> { new MockParameter("level", "Brightness level from 0 to 100") }),
                        "Returns current brightness level.",
                        "{\"type\": \"object\", \"properties\": {\"brightness\": {\"type\": \"integer\", \"minimum\": 0, \"maximum\": 100}}}"
                    )
                }
            )
        };
    }


    public async Task<List<MockDevice>> GetDevicesAsync()
    {
        await LoadDataAsync(); // Ensure fresh data
        return _devices;
    }

    public async Task<MockDevice?> GetDeviceByIdAsync(string id)
    {
        await LoadDataAsync();
        return _devices.FirstOrDefault(d => d.Identifier.Equals(id, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<MockDevice> AddDeviceAsync(MockDevice device)
    {
        await LoadDataAsync();
        // Simple check for duplicate identifier, real app might need more robust handling
        if (_devices.Any(d => d.Identifier.Equals(device.Identifier, StringComparison.OrdinalIgnoreCase)))
        {
            // In a real scenario, might throw an exception or return a specific error
            // For mock, we'll overwrite if ID is manually set and exists, or ensure unique if auto-generated
            // The endpoint logic already generates a new GUID if identifier is null/empty
            var existing = _devices.FirstOrDefault(d => d.Identifier.Equals(device.Identifier, StringComparison.OrdinalIgnoreCase));
            if (existing != null) _devices.Remove(existing);
        }
        _devices.Add(device);
        await SaveDataAsync();
        return device;
    }

    public async Task<MockDevice?> UpdateDeviceAsync(MockDevice device)
    {
        await LoadDataAsync();
        var index = _devices.FindIndex(d => d.Identifier.Equals(device.Identifier, StringComparison.OrdinalIgnoreCase));
        if (index == -1) return null;

        _devices[index] = device;
        await SaveDataAsync();
        return device;
    }

    public async Task<MockDevice?> DeleteDeviceAsync(string id)
    {
        await LoadDataAsync();
        var device = _devices.FirstOrDefault(d => d.Identifier.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (device == null) return null;

        _devices.Remove(device);
        await SaveDataAsync();
        return device;
    }

    private class DeviceDataWrapper
    {
        public List<MockDevice> Devices { get; set; } = new();
    }
}

// This line is necessary for tests or other assemblies to access the internal types if needed.
// However, for a self-contained mock API executable, it's not strictly required.
// For the sake of completeness if you were to add integration tests in a separate project:
public partial class Program { }