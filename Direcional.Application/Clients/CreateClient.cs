using Direcional.Application.Abstractions;
using Direcional.Domain.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Direcional.Application.Clients;

public static class CreateClient
{
    public record Command(string Name, string Email, string Document, string? Phone) : IRequest<Result>;
    public record Result(Guid Id);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
            RuleFor(x => x.Document).NotEmpty().MaximumLength(20);
            RuleFor(x => x.Phone).MaximumLength(20);
        }
    }

    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var exists = await _db.Clients.AnyAsync(c => c.Email == request.Email || c.Document == request.Document, cancellationToken);
            if (exists) throw new InvalidOperationException("Client with same email or document already exists");

            var entity = new Client
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Email = request.Email,
                Document = request.Document,
                Phone = request.Phone ?? string.Empty
            };

            _db.Clients.Add(entity);
            await _db.SaveChangesAsync(cancellationToken);
            return new Result(entity.Id);
        }
    }
}


