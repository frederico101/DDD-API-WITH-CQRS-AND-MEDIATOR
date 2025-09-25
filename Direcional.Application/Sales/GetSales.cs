using Direcional.Application.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Direcional.Application.Sales;

public static class GetSales
{
    public record Query(int Page, int PageSize) : IRequest<PagedResult<SaleDto>>;

    public record SaleDto(
        Guid Id,
        Guid ClientId,
        string ClientName,
        Guid ApartmentId,
        string ApartmentCode,
        decimal DownPayment,
        decimal TotalPrice,
        DateTime CreatedAtUtc
    );

    public record PagedResult<T>(int Total, List<T> Items);

    public class Handler(IAppDbContext context) : IRequestHandler<Query, PagedResult<SaleDto>>
    {
        public async Task<PagedResult<SaleDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var query = context.Sales.AsQueryable();

            var total = await query.CountAsync(cancellationToken);

            var sales = await query
                .OrderByDescending(s => s.SoldAtUtc)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            // Get client and apartment names separately
            var clientIds = sales.Select(s => s.ClientId).Distinct().ToList();
            var apartmentIds = sales.Select(s => s.ApartmentId).Distinct().ToList();

            var clients = await context.Clients
                .Where(c => clientIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c.Name, cancellationToken);

            var apartments = await context.Apartments
                .Where(a => apartmentIds.Contains(a.Id))
                .ToDictionaryAsync(a => a.Id, a => a.Code, cancellationToken);

            var saleDtos = sales.Select(s => new SaleDto(
                s.Id,
                s.ClientId,
                clients.GetValueOrDefault(s.ClientId, "Unknown"),
                s.ApartmentId,
                apartments.GetValueOrDefault(s.ApartmentId, "Unknown"),
                s.DownPayment,
                s.TotalPrice,
                s.SoldAtUtc
            )).ToList();

            return new PagedResult<SaleDto>(total, saleDtos);
        }
    }
}
