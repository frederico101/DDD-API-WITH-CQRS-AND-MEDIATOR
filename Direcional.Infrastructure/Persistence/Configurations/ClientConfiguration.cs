using Direcional.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Direcional.Infrastructure.Persistence.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("clients");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Document).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Phone).HasMaxLength(20);
        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(x => x.Document).IsUnique();
    }
}


