using System;

namespace Direcional.Domain.Entities;

public class Reservation
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public Guid ApartmentId { get; set; }
    public DateTime ReservedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAtUtc { get; set; }
    public bool ConfirmedAsSale { get; set; }
}


