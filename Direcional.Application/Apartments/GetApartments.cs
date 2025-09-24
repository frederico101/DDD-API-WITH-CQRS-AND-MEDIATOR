using Direcional.Application.Abstractions;
using Direcional.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Direcional.Application.Apartments;

public static class GetApartments
{
    public record Query(int Page = 1, int PageSize = 20, string? Search = null) : IRequest<Result>;
    public record Result(int Total, IEnumerable<Apartment> Items);

    public class Handler : IRequestHandler<Query, Result>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task<Result> Handle(Query request, CancellationToken cancellationToken)
        {
            var q = _db.Apartments.AsQueryable();
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var s = request.Search.ToLower();
                q = q.Where(a => a.Code.ToLower().Contains(s) || a.Block.ToLower().Contains(s));
            }
            var total = await q.CountAsync(cancellationToken);
            var items = await q.OrderBy(a => a.Code)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);
            return new Result(total, items);
        }
    }
}


