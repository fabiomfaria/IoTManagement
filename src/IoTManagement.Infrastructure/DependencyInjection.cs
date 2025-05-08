using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IoTManagement.Domain.Interfaces;
using IoTManagement.Domain.Repositories;
using IoTManagement.Domain.Services;
using IoTManagement.Infrastructure.Repositories;
using IoTManagement.Infrastructure.Services;

namespace IoTManagement.Infrastructure
{
    /// <summary>
    /// Configuração de injeção de dependências para a camada de infraestrutura
    /// </summary>
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Registrar repositórios
            services.AddHttpClient<IDeviceRepository, CIoTDDeviceRepository>();
            
            // Registrar serviços
            services.AddSingleton<IUserStore, UserStore>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<ITelnetClient, TelnetClient>();
            
            // Registrar serviços de autenticação
            services.AddScoped<IoTManagement.Application.Interfaces.IAuthService, AuthService>();
            
            return services;
        }
    }
}