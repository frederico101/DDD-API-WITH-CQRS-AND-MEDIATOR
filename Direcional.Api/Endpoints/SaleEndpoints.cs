using Direcional.Application.Sales;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace Direcional.Api.Endpoints;

public static class SaleEndpoints
{
    public static IEndpointRouteBuilder MapSales(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/sales").RequireAuthorization();

        group.MapGet("/", async (int page, int pageSize, ISender sender) =>
        {
            var result = await sender.Send(new GetSales.Query(page, pageSize));
            return Results.Ok(result);
        })
        .WithTags("Sales")
        .WithOpenApi();

        group.MapPost("/", async (ConfirmSale.ConfirmSaleCommand cmd, ISender sender) =>
        {
            var result = await sender.Send(cmd);
            return Results.Created($"/sales/{result.Id}", result);
        })
        .WithTags("Sales")
        .WithOpenApi();

        return app;
    }
}


