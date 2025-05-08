using IoTManagement.API.Interfaces;
using IoTManagement.Application.Interfaces;
using IoTManagement.Application.Services;
using IoTManagement.Domain.Interfaces;
using IoTManagement.Domain.Repositories;
using IoTManagement.Domain.Services;
using IoTManagement.Infrastructure.Repositories;
using IoTManagement.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace IoTManagement.API.Extensions
{
    /// <summary>
    /// Extensões para configuração de serviços da aplicação
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Configura serviços da API
        /// </summary>
        public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Adiciona controllers e API Explorer
            services.AddControllers();
            services.AddEndpointsApiExplorer();
            
            // Configura CORS
            services.AddCors(options =>
            {
                options.AddPolicy("AllowBlazorApp", policy =>
                {
                    policy.WithOrigins(configuration.GetSection("AllowedOrigins").Get<string[]>())
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });
            
            // Configura Swagger
            services.AddSwaggerConfiguration();
            
            // Configura autenticação e autorização
            services.AddAuthenticationConfiguration(configuration);
            
            return services;
        }
        
        /// <summary>
        /// Configura Swagger
        /// </summary>
        public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "IoTManagement API",
                    Version = "v1",
                    Description = "API para gerenciamento de dispositivos IoT",
                    Contact = new OpenApiContact
                    {
                        Name = "Equipe de Desenvolvimento",
                        Email = "dev@iotmanagement.com"
                    }
                });

                // Configuração para documentação XML
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }

                // Configuração de autenticação OAuth2 no Swagger
                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        Password = new OpenApiOAuthFlow
                        {
                            TokenUrl = new Uri("/api/auth/token", UriKind.Relative),
                            Scopes = new Dictionary<string, string>
                            {
                                { "api", "Acesso à API IoT Management" }
                            }
                        }
                    }
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "oauth2"
                            }
                        },
                        new[] { "api" }
                    }
                });
            });
            
            return services;
        }
        
        /// <summary>
        /// Configura autenticação e autorização
        /// </summary>
        public static IServiceCollection AddAuthenticationConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration["OAuth:SecretKey"] ?? "DefaultSecretKeyWithAtLeast32Characters!!")),
                ValidateIssuer = true,
                ValidIssuer = configuration["OAuth:Authority"],
                ValidateAudience = true,
                ValidAudience = configuration["OAuth:ClientId"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = tokenValidationParameters;
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("ApiScope", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("scope", "api");
                });
                
                options.AddPolicy("AdminOnly", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim("role", "Admin");
                });
            });
            
            return services;
        }
        
        /// <summary>
        /// Registra serviços da aplicação
        /// </summary>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Registra serviços da camada de aplicação
            services.AddScoped<IAuthService, Application.Services.AuthService>();
            services.AddScoped<Application.Services.IDeviceService, Application.Services.DeviceService>();
            services.AddScoped<ICommandService, CommandService>();
            
            // Registra serviços da camada de domínio
            services.AddScoped<Domain.Interfaces.ITokenService, TokenService>();
            services.AddScoped<IUserStore, UserStore>();
            services.AddScoped<ITelnetClient, TelnetClient>();
            services.AddScoped<IDeviceCommandExecutionService, DeviceCommandExecutionService>();
            
            // Registra repositórios
            services.AddScoped<IDeviceRepository, CIoTDDeviceRepository>();
            
            // Configura HttpClient para a API CIoTD
            services.AddHttpClient<ICIoTDApiService, CIoTDApiService>(client =>
            {
                client.BaseAddress = new Uri(configuration["CIoTDApi:BaseUrl"]);
            });
            
            return services;
        }
    }
}