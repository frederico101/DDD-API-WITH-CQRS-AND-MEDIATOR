using Direcional.Application.Abstractions;
using Direcional.Domain.Entities;
using Direcional.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Direcional.Application.Reservations;

public static class CreateReservation
{
    public record Command(Guid ClientId, Guid ApartmentId, int ExpiresHours) : IRequest<Result>;
    public record Result(Guid Id);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.ClientId).NotEmpty();
            RuleFor(x => x.ApartmentId).NotEmpty();
            RuleFor(x => x.ExpiresHours).GreaterThan(0).LessThanOrEqualTo(168);
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
            return new Result(reservation.Id);
        }
    }
}


