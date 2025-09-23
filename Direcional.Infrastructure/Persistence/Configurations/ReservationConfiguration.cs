using Direcional.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Direcional.Infrastructure.Persistence.Configurations;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("reservas");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ReservedAtUtc).IsRequired();
        builder.HasIndex(x => new { x.ClientId, x.ApartmentId }).IsUnique(false);
    }
}


