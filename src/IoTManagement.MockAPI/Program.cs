using IoTManagement.Domain.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure custom basic authentication
builder.Services.AddAuthentication("BasicAuthentication")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);
builder.Services.AddAuthorization();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// Setup mock data
var mockDevices = new List<Device>
{
    new Device
    {
        Identifier = "device001",
        Description = "Sensor de temperatura e umidade para uso agrícola",
        Manufacturer = "TechFarm Inc.",
        Url = "device001.iot.local",
        Commands = new List<CommandDescription>
        {
            new CommandDescription
            {
                Operation = "GetTemperature",
                Description = "Obtém a temperatura atual do ambiente em graus Celsius",
                Command = new Command
                {
                    CommandText = "gettemp",
                    Parameters = new List<Parameter>()
                },
                Result = "Temperatura atual em graus Celsius",
                Format = "{ \"temperature\": { \"type\": \"number\" } }"
            },
            new CommandDescription
            {
                Operation = "GetHumidity",
                Description = "Obtém a umidade relativa do ar em percentual",
                Command = new Command
                {
                    CommandText = "gethum",
                    Parameters = new List<Parameter>()
                },
                Result = "Umidade relativa do ar em percentual",
                Format = "{ \"humidity\": { \"type\": \"number\" } }"
            },
            new CommandDescription
            {
                Operation = "SetMeasurementInterval",
                Description = "Define o intervalo de medição em segundos",
                Command = new Command
                {
                    CommandText = "setinterval",
                    Parameters = new List<Parameter>
                    {
                        new Parameter
                        {
                            Name = "interval",
                            Description = "Intervalo de medição em segundos (entre 5 e 3600)"
                        }
                    }
                },
                Result = "Status da configuração do intervalo",
                Format = "{ \"status\": { \"type\": \"string\" } }"
            }
        }
    },
    new Device
    {
        Identifier = "device002",
        Description = "Controlador de irrigação automática",
        Manufacturer = "SmartIrrigation Co.",
        Url = "device002.iot.local",
        Commands = new List<CommandDescription>
        {
            new CommandDescription
            {
                Operation = "GetWaterLevel",
                Description = "Obtém o nível atual do reservatório de água em percentual",
                Command = new Command
                {
                    CommandText = "getwaterlevel",
                    Parameters = new List<Parameter>()
                },
                Result = "Nível de água em percentual",
                Format = "{ \"waterLevel\": { \"type\": \"number\" } }"
            },
            new CommandDescription
            {
                Operation = "StartIrrigation",
                Description = "Inicia a irrigação por um período específico",
                Command = new Command
                {
                    CommandText = "startirrig",
                    Parameters = new List<Parameter>
                    {
                        new Parameter
                        {
                            Name = "duration",
                            Description = "Duração da irrigação em minutos (entre 1 e 60)"
                        },
                        new Parameter
                        {
                            Name = "zone",
                            Description = "Zona de irrigação (1-5)"
                        }
                    }
                },
                Result = "Status da operação de irrigação",
                Format = "{ \"status\": { \"type\": \"string\" }, \"message\": { \"type\": \"string\" } }"
            },
            new CommandDescription
            {
                Operation = "StopIrrigation",
                Description = "Interrompe a irrigação em andamento",
                Command = new Command
                {
                    CommandText = "stopirrig",
                    Parameters = new List<Parameter>
                    {
                        new Parameter
                        {
                            Name = "zone",
                            Description = "Zona de irrigação (1-5)"
                        }
                    }
                },
                Result = "Status da operação de interrupção",
                Format = "{ \"status\": { \"type\": \"string\" } }"
            }
        }
    }
};

// API Endpoints
// GET /device - Get all device identifiers
app.MapGet("/api/device", () =>
{
    var deviceIds = mockDevices.Select(d => d.Identifier).ToList();
    return Results.Ok(deviceIds);
}).RequireAuthorization();

// POST /device - Create a new device
app.MapPost("/api/device", (Device device) =>
{
    // Validate device data
    if (string.IsNullOrEmpty(device.Identifier) || string.IsNullOrEmpty(device.Description))
    {
        return Results.BadRequest("Device identifier and description are required");
    }

    // Check if device already exists
    if (mockDevices.Any(d => d.Identifier == device.Identifier))
    {
        return Results.Conflict($"Device with identifier '{device.Identifier}' already exists");
    }

    // Add the device
    mockDevices.Add(device);
    return Results.Created($"/api/device/{device.Identifier}", device);
}).RequireAuthorization();

// GET /device/{id} - Get device details
app.MapGet("/api/device/{id}", (string id) =>
{
    var device = mockDevices.FirstOrDefault(d => d.Identifier == id);
    if (device == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(device);
}).RequireAuthorization();

// PUT /device/{id} - Update device
app.MapPut("/api/device/{id}", (string id, Device updatedDevice) =>
{
    var existingDeviceIndex = mockDevices.FindIndex(d => d.Identifier == id);
    if (existingDeviceIndex == -1)
    {
        return Results.NotFound();
    }

    // Update the device
    mockDevices[existingDeviceIndex] = updatedDevice;
    return Results.Ok(updatedDevice);
}).RequireAuthorization();

// DELETE /device/{id} - Delete device
app.MapDelete("/api/device/{id}", (string id) =>
{
    var existingDevice = mockDevices.FirstOrDefault(d => d.Identifier == id);
    if (existingDevice == null)
    {
        return Results.NotFound();
    }

    mockDevices.Remove(existingDevice);
    return Results.Ok(existingDevice);
}).RequireAuthorization();

// Start the mock API
app.Run("http://localhost:5005");

// Basic Authentication Handler
public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public BasicAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock)
    { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Skip authentication for development convenience
        var claims = new[] { new Claim(ClaimTypes.Name, "mock-user") };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
