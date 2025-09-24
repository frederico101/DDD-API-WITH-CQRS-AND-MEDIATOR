using Direcional.Application.Abstractions;
using Direcional.Domain.Entities;
using Direcional.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Direcional.Application.Sales;

public static class ConfirmSale
{
    public record Command(Guid ClientId, Guid ApartmentId, Guid? ReservationId, decimal DownPayment, decimal TotalPrice) : IRequest<Result>;
    public record Result(Guid Id);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.ClientId).NotEmpty();
            RuleFor(x => x.ApartmentId).NotEmpty();
            RuleFor(x => x.TotalPrice).GreaterThan(0);
            RuleFor(x => x.DownPayment).GreaterThanOrEqualTo(0);
        }
    }

    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var apartment = await _db.Apartments.FirstOrDefaultAsync(a => a.Id == request.ApartmentId, cancellationToken)
                            ?? throw new KeyNotFoundException("Apartment not found");
            if (apartment.Status == ApartmentStatus.Sold)
                throw new InvalidOperationException("Apartment already sold");

            Reservation? reservation = null;
            if (request.ReservationId.HasValue)
            {
                reservation = await _db.Reservations.FirstOrDefaultAsync(r => r.Id == request.ReservationId.Value, cancellationToken)
                              ?? throw new KeyNotFoundException("Reservation not found");
            }

            var sale = new Sale
            {
                Id = Guid.NewGuid(),
                ClientId = request.ClientId,
                ApartmentId = request.ApartmentId,
                ReservationId = request.ReservationId,
                DownPayment = request.DownPayment,
                TotalPrice = request.TotalPrice
            };
            _db.Sales.Add(sale);

            apartment.Status = ApartmentStatus.Sold;
            if (reservation is not null) reservation.ConfirmedAsSale = true;

            await _db.SaveChangesAsync(cancellationToken);
            return new Result(sale.Id);
        }
    }
}


