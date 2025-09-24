using Direcional.Application.Abstractions;
using Direcional.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Direcional.Application.Reservations;

public static class CancelReservation
{
    public record CancelReservationCommand(Guid ReservationId) : IRequest<Unit>;

    public class Handler : IRequestHandler<CancelReservationCommand, Unit>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task<Unit> Handle(CancelReservationCommand request, CancellationToken cancellationToken)
        {
            var reservation = await _db.Reservations.FirstOrDefaultAsync(r => r.Id == request.ReservationId, cancellationToken)
                               ?? throw new KeyNotFoundException("Reservation not found");
            if (reservation.ConfirmedAsSale)
                throw new InvalidOperationException("Reservation already confirmed as sale");

            var apartment = await _db.Apartments.FirstAsync(a => a.Id == reservation.ApartmentId, cancellationToken);
            _db.Reservations.Remove(reservation);
            apartment.Status = ApartmentStatus.Available;
            await _db.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}


