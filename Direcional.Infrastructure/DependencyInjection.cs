using Direcional.Application.Abstractions;
using Direcional.Infrastructure.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Direcional.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SqlServer");

        services.AddDbContext<AppDbContext>(options =>
        {
            var env = configuration["ASPNETCORE_ENVIRONMENT"];
            if (string.Equals(env, "Testing", StringComparison.OrdinalIgnoreCase))
            {
                options.UseInMemoryDatabase("TestDb");
            }
            else
            {
                options.UseSqlServer(connectionString);
            }
        });

        services.AddHealthChecks();

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            // Register consumers so queues are created and messages are visible in RabbitMQ
            x.AddConsumers(typeof(DependencyInjection).Assembly);

            x.UsingRabbitMq((context, cfg) =>
            {
                var host = configuration["RabbitMQ:Host"] ?? "rabbitmq";
                cfg.Host(host, "/", h => { });

                // Auto-configure receive endpoints for all registered consumers
                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        return services;
    }
}


