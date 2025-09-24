using Direcional.Application.Abstractions;
using Direcional.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Direcional.Application.Clients;

public static class GetClients
{
    public record Query(int Page = 1, int PageSize = 20, string? Search = null) : IRequest<Result>;
    public record Result(int Total, IEnumerable<Client> Items);

    public class Handler : IRequestHandler<Query, Result>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task<Result> Handle(Query request, CancellationToken cancellationToken)
        {
            var q = _db.Clients.AsQueryable();
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var s = request.Search.ToLower();
                q = q.Where(c => c.Name.ToLower().Contains(s) || c.Email.ToLower().Contains(s) || c.Document.Contains(s));
            }
            var total = await q.CountAsync(cancellationToken);
            var items = await q.OrderBy(c => c.Name)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);
            return new Result(total, items);
        }
    }
}


