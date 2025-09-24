using Direcional.Application.Abstractions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Direcional.Application.Apartments;

public static class UpdateApartment
{
    public record Command(Guid Id, string Code, string Block, int Floor, int Number, decimal Price) : IRequest<Unit>;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Block).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Floor).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Number).GreaterThan(0);
            RuleFor(x => x.Price).GreaterThan(0);
        }
    }

    public class Handler : IRequestHandler<Command, Unit>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            var entity = await _db.Apartments.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
                         ?? throw new KeyNotFoundException("Apartment not found");

            var clash = await _db.Apartments.AnyAsync(a => a.Code == request.Code && a.Id != request.Id, cancellationToken);
            if (clash) throw new InvalidOperationException("Another apartment with same code exists");

            entity.Code = request.Code;
            entity.Block = request.Block;
            entity.Floor = request.Floor;
            entity.Number = request.Number;
            entity.Price = request.Price;
            await _db.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}


