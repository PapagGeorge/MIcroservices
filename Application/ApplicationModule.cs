using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Application.Interfaces;
using Application.Services;
using Infrastructure.DTOs;
using Microsoft.Extensions.Logging;

namespace Application;

public static class ApplicationModule
{
    public static IServiceCollection AddServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register application services
        services.AddScoped<IRecipeService, RecipeService>();
        services.AddScoped(typeof(IRabbitMqRpcClient<,>), typeof(RabbitMqRpcClient<,>));
        // In Program.cs or Startup.cs
        services.AddSingleton<IRabbitMqRpcClient<string, NutritionResponseDto>>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<RabbitMqRpcClient<string, NutritionResponseDto>>>();
            var configuration = provider.GetRequiredService<IConfiguration>();

            // Read from configuration with fallbacks
            string host = configuration["RabbitMQ:Host"] ?? "localhost";
            string username = configuration["RabbitMQ:Username"] ?? "guest";
            string password = configuration["RabbitMQ:Password"] ?? "guest";
            int port = int.TryParse(configuration["RabbitMQ:Port"], out var p) ? p : 5672;

            return new RabbitMqRpcClient<string, NutritionResponseDto>(
                logger, host, username, password, port);
        });
        return services;
    }
}
