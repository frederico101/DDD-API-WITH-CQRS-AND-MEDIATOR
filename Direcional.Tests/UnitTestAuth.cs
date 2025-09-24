using System;
using System.Threading;
using System.Threading.Tasks;
using Direcional.Application.Auth;
using Direcional.Application.Abstractions;
using Direcional.Domain.Entities;
using Direcional.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Direcional.Tests;

public class UnitTestAuth
{
    private static AppDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task Login_Generates_JWT_For_Valid_User()
    {
        using var db = CreateInMemoryDb();
        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Role = "Admin"
        });
        await db.SaveChangesAsync();

        var inMemory = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string,string>("Jwt:Issuer", "Direcional"),
                new KeyValuePair<string,string>("Jwt:Audience", "DirecionalAudience"),
                new KeyValuePair<string,string>("Jwt:Key", new string('K', 32))
            })
            .Build();

        IAppDbContext appDb = db;
        var handler = new Login.Handler(appDb, inMemory);

        var result = await handler.Handle(new Login.LoginCommand("admin", "admin123"), CancellationToken.None);

        result.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.AccessToken.Split('.').Length.Should().Be(3);
    }
}