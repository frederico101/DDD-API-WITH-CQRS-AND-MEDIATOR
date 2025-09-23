using System;

namespace Direcional.Domain.Entities;

public class Sale
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public Guid ApartmentId { get; set; }
    public Guid? ReservationId { get; set; }
    public decimal DownPayment { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime SoldAtUtc { get; set; } = DateTime.UtcNow;
}


