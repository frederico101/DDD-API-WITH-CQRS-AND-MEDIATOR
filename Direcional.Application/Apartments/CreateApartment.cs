using Direcional.Application.Abstractions;
using Direcional.Domain.Entities;
using Direcional.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Direcional.Application.Apartments;

public static class CreateApartment
{
    public record CreateApartmentCommand(string Code, string Block, int Floor, int Number, decimal Price) : IRequest<CreateApartmentResult>;
    public record CreateApartmentResult(Guid Id);

    public class Validator : AbstractValidator<CreateApartmentCommand>
    {
        public Validator()
        {
            RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Block).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Floor).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Number).GreaterThan(0);
            RuleFor(x => x.Price).GreaterThan(0);
        }
    }

    public class Handler : IRequestHandler<CreateApartmentCommand, CreateApartmentResult>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task<CreateApartmentResult> Handle(CreateApartmentCommand request, CancellationToken cancellationToken)
        {
            if (await _db.Apartments.AnyAsync(a => a.Code == request.Code, cancellationToken))
                throw new InvalidOperationException("Apartment code already exists");

            var entity = new Apartment
            {
                Id = Guid.NewGuid(),
                Code = request.Code,
                Block = request.Block,
                Floor = request.Floor,
                Number = request.Number,
                Price = request.Price,
                Status = ApartmentStatus.Available
            };

            _db.Apartments.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);
            return new CreateApartmentResult(entity.Id);
        }
    }
}


