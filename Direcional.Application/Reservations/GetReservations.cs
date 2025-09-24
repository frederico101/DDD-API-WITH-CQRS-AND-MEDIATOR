using Direcional.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Direcional.Application.Reservations;

public static class GetReservations
{
    public record Query(int Page = 1, int PageSize = 50) : IRequest<Result>;
    public record Item(Guid Id, Guid ClientId, string ClientName, Guid ApartmentId, string ApartmentCode, DateTime? ExpiresAtUtc, bool ConfirmedAsSale);
    public record Result(int Total, IEnumerable<Item> Items);

    public class Handler : IRequestHandler<Query, Result>
    {
        private readonly IAppDbContext _db;
        public Handler(IAppDbContext db) => _db = db;

        public async Task<Result> Handle(Query request, CancellationToken cancellationToken)
        {
            var q = from r in _db.Reservations
                    join c in _db.Clients on r.ClientId equals c.Id
                    join a in _db.Apartments on r.ApartmentId equals a.Id
                    orderby r.ReservedAtUtc descending
                    select new Item(r.Id, r.ClientId, c.Name, r.ApartmentId, a.Code, r.ExpiresAtUtc, r.ConfirmedAsSale);

            var total = await q.CountAsync(cancellationToken);
            var items = await q.Skip((request.Page - 1) * request.PageSize)
                               .Take(request.PageSize)
                               .ToListAsync(cancellationToken);
            return new Result(total, items);
        }
    }
}


