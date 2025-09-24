using System.Linq;
using Direcional.Infrastructure.Persistence;
using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Direcional.Tests.Integration;

public class TestWebAppFactory : WebApplicationFactory<global::Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            // Replace DbContext with InMemory
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
            services.RemoveAll(typeof(AppDbContext));
            services.RemoveAll(typeof(Direcional.Application.Abstractions.IAppDbContext));

            services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("TestDb"));
            services.AddScoped<Direcional.Application.Abstractions.IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

            // Replace MassTransit with in-memory transport
            var mtDescriptors = services.Where(d => d.ServiceType.Namespace != null && d.ServiceType.Namespace.Contains("MassTransit")).ToList();
            foreach (var d in mtDescriptors) services.Remove(d);

            services.AddMassTransit(x =>
            {
                x.UsingInMemory((context, cfg) => { cfg.ConfigureEndpoints(context); });
            });

            // Seed basic data for tests
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
            Seed.SeedAdminUser(db);
            Seed.SeedApartments(db);
            Seed.SeedDefaultClient(db);
        });
    }
}


