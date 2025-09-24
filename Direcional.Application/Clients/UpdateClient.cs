using Direcional.Application.Abstractions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Direcional.Application.Clients;

public static class UpdateClient
{
    public record Command(Guid Id, string Name, string Email, string Document, string? Phone) : IRequest;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
            RuleFor(x => x.Document).NotEmpty().MaximumLength(20);
            RuleFor(x => x.Phone).MaximumLength(20);
        }
    }

    public class Handler : IRequestHandler<Command, Unit>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            var entity = await _db.Clients.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
                         ?? throw new KeyNotFoundException("Client not found");

            var emailDocClash = await _db.Clients.AnyAsync(c => (c.Email == request.Email || c.Document == request.Document) && c.Id != request.Id, cancellationToken);
            if (emailDocClash) throw new InvalidOperationException("Another client with same email or document already exists");

            entity.Name = request.Name;
            entity.Email = request.Email;
            entity.Document = request.Document;
            entity.Phone = request.Phone ?? string.Empty;
            await _db.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}


