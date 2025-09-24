using Direcional.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Direcional.Application.Clients;

public static class DeleteClient
{
    public record DeleteClientCommand(Guid Id) : IRequest;

    public class Handler : IRequestHandler<DeleteClientCommand, Unit>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task<Unit> Handle(DeleteClientCommand request, CancellationToken cancellationToken)
        {
            var entity = await _db.Clients.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
                         ?? throw new KeyNotFoundException("Client not found");
            _db.Clients.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}


