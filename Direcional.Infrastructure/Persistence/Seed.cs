using System;
using System.Linq;
using BCrypt.Net;
using Direcional.Domain.Entities;
using MassTransit;

namespace Direcional.Infrastructure.Persistence;

public static class Seed
{
    public static void SeedAdminUser(AppDbContext db)
    {
        if (!db.Users.Any())
        {
            var admin = new User
            {
                Id = Guid.NewGuid(),
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Role = "Admin"
            };
            db.Users.Add(admin);
            db.SaveChanges();
        }
    }

    public static void SeedApartments(AppDbContext db)
    {
        if (!db.Apartments.Any())
        {
            var items = new[]
            {
                new Apartment { Id = Guid.NewGuid(), Code = "A-101", Block = "A", Floor = 1, Number = 101, Price = 350000m },
                new Apartment { Id = Guid.NewGuid(), Code = "A-102", Block = "A", Floor = 1, Number = 102, Price = 355000m },
                new Apartment { Id = Guid.NewGuid(), Code = "B-201", Block = "B", Floor = 2, Number = 201, Price = 410000m }
            };
            db.Apartments.AddRange(items);
            db.SaveChanges();
        }
    }

    public static void SeedDefaultClient(AppDbContext db)
    {
        const string email = "frederico.alves@example.com";
        if (!db.Clients.Any(c => c.Email == email))
        {
            var client = new Client
            {
                Id = Guid.NewGuid(),
                Name = "Frederico Alves",
                Email = email,
                Document = "12345678900",
                Phone = "+55 11 99999-9999"
            };
            db.Clients.Add(client);
            db.SaveChanges();
        }
    }
}


