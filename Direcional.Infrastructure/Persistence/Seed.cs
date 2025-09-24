using System;
using System.Linq;
using BCrypt.Net;
using Direcional.Domain.Entities;

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
}


