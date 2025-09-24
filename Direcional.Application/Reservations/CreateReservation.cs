using Direcional.Application.Abstractions;
using Direcional.Domain.Entities;
using Direcional.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MassTransit;

namespace Direcional.Application.Reservations;

public static class CreateReservation
{
    public record CreateReservationCommand(Guid ClientId, Guid ApartmentId, int ExpiresHours) : IRequest<CreateReservationResult>;
    public record CreateReservationResult(Guid Id);

    public class Validator : AbstractValidator<CreateReservationCommand>
    {
        public Validator()
        {
            RuleFor(x => x.ClientId).NotEmpty();
            RuleFor(x => x.ApartmentId).NotEmpty();
            RuleFor(x => x.ExpiresHours).GreaterThan(0).LessThanOrEqualTo(168);
        }
    }

    public class Handler : IRequestHandler<CreateReservationCommand, CreateReservationResult>
    {
        private readonly IAppDbContext _db;
        private readonly IPublishEndpoint _publisher;
        public Handler(IAppDbContext db, IPublishEndpoint publisher)
        {
            _db = db;
            _publisher = publisher;
        }

        public async Task<CreateReservationResult> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
        {
            var apartment = await _db.Apartments.FirstOrDefaultAsync(a => a.Id == request.ApartmentId, cancellationToken)
                            ?? throw new KeyNotFoundException("Apartment not found");
            if (apartment.Status != ApartmentStatus.Available)
                throw new InvalidOperationException("Apartment not available for reservation");

            var reservation = new Reservation
            {
                Id = Guid.NewGuid(),
                ClientId = request.ClientId,
                ApartmentId = request.ApartmentId,
                ExpiresAtUtc = DateTime.UtcNow.AddHours(request.ExpiresHours),
                ConfirmedAsSale = false
            };
            _db.Reservations.Add(reservation);
            apartment.Status = ApartmentStatus.Reserved;
            await _db.SaveChangesAsync(cancellationToken);
            await _publisher.Publish(new ReservationCreated(reservation.Id, reservation.ClientId, reservation.ApartmentId, reservation.ExpiresAtUtc), cancellationToken);
            return new CreateReservationResult(reservation.Id);
        }
    }
}

public record ReservationCreated(Guid ReservationId, Guid ClientId, Guid ApartmentId, DateTime? ExpiresAtUtc);


