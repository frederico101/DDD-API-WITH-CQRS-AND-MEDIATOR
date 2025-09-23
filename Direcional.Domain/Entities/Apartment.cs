using System;
using Direcional.Domain.Enums;

namespace Direcional.Domain.Entities;

public class Apartment
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty; // unique code/identifier
    public string Block { get; set; } = string.Empty;
    public int Floor { get; set; }
    public int Number { get; set; }
    public decimal Price { get; set; }
    public ApartmentStatus Status { get; set; } = ApartmentStatus.Available;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}


