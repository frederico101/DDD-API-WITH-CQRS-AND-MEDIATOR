using Direcional.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Direcional.Application.Apartments;

public static class DeleteApartment
{
    public record Command(Guid Id) : IRequest<Unit>;

    public class Handler : IRequestHandler<Command, Unit>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            var entity = await _db.Apartments.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
                         ?? throw new KeyNotFoundException("Apartment not found");
            _db.Apartments.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}


