using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Direcional.Domain.Entities;

namespace Direcional.Application.Abstractions;

public interface IAppDbContext
{
    DbSet<Client> Clients { get; }
    DbSet<Apartment> Apartments { get; }
    DbSet<Reservation> Reservations { get; }
    DbSet<Sale> Sales { get; }
    DbSet<User> Users { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}


