using System;
using System.Net.Http;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using IoTManagement.UI.Blazor;
using IoTManagement.UI.Blazor.Services;
using IoTManagement.UI.Blazor.Providers;
using Blazored.LocalStorage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.Configuration["API:BaseUrl"] ?? "https://localhost:7001") });

// Add auth services
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();
builder.Services.AddAuthorizationCore();

// Add device services
builder.Services.AddScoped<DeviceService>();
builder.Services.AddScoped<CommandExecutionService>();

await builder.Build().RunAsync();
