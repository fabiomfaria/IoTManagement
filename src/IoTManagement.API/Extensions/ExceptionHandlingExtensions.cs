using IoTManagement.API.Middlewares;
using IoTManagement.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using System.Text.Json;

namespace IoTManagement.API.Extensions
{
    /// <summary>
    /// Extensões para configuração de manipulação de exceções
    /// </summary>
    public static class ExceptionHandlingExtensions
    {
        /// <summary>
        /// Configura o middleware de tratamento de exceções
        /// </summary>
        public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Usar middleware customizado para tratamento de exceções
            app.UseMiddleware<ExceptionMiddleware>();
            
            return app;
        }

        /// <summary>
        /// Adiciona um manipulador global de exceções
        /// </summary>
        public static void AddGlobalExceptionHandler(this WebApplication app, IWebHostEnvironment env)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";

                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                    if (contextFeature != null)
                    {
                        // Determinar o status code apropriado baseado no tipo de exceção
                        context.Response.StatusCode = contextFeature.Error switch
                        {
                            KeyNotFoundException => (int)HttpStatusCode.NotFound,
                            ValidationException => (int)HttpStatusCode.BadRequest,
                            UnauthorizedException => (int)HttpStatusCode.Unauthorized,
                            TelnetCommunicationException => (int)HttpStatusCode.BadGateway,
                            _ => (int)HttpStatusCode.InternalServerError
                        };

                        // Construir e retornar o erro
                        var response = new
                        {
                            status = context.Response.StatusCode,
                            message = contextFeature.Error.Message,
                            detail = env.IsDevelopment() ? contextFeature.Error.StackTrace : null
                        };

                        var options = new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        };

                        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
                    }
                });
            });
        }

        /// <summary>
        /// Configura o middleware de tratamento de exceções
        /// </summary>
        public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseExceptionHandler(appBuilder =>
            {
                appBuilder.Run(async context =>
                {
                    context.Response.ContentType = "application/json";
                    
                    var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                    var exception = exceptionHandlerPathFeature?.Error;
                    
                    if (exception != null)
                    {
                        // Definir o status code apropriado
                        var statusCode = exception switch
                        {
                            KeyNotFoundException => HttpStatusCode.NotFound,
                            ValidationException => HttpStatusCode.BadRequest,
                            UnauthorizedException => HttpStatusCode.Unauthorized,
                            TelnetCommunicationException => HttpStatusCode.BadGateway,
                            _ => HttpStatusCode.InternalServerError
                        };
                        
                        context.Response.StatusCode = (int)statusCode;
                        
                        var response = new
                        {
                            StatusCode = context.Response.StatusCode,
                            Message = exception.Message,
                            Detail = env.IsDevelopment() ? exception.StackTrace : null
                        };
                        
                        var options = new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        };
                        
                        var json = JsonSerializer.Serialize(response, options);
                        await context.Response.WriteAsync(json);
                    }
                });
            });
            
            return app;
        }
    }
}