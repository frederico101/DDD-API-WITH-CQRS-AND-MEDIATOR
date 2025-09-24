using Direcional.Application.Apartments;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace Direcional.Api.Endpoints;

public static class ApartmentEndpoints
{
    public static IEndpointRouteBuilder MapApartments(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/apartments").RequireAuthorization();

        group.MapGet("/", async (int page, int pageSize, string? search, ISender sender) =>
        {
            var result = await sender.Send(new GetApartments.Query(page, pageSize, search));
            return Results.Ok(result);
        });

        group.MapPost("/", async (CreateApartment.Command cmd, ISender sender) =>
        {
            var result = await sender.Send(cmd);
            return Results.Created($"/apartments/{result.Id}", result);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateApartment.Command body, ISender sender) =>
        {
            var cmd = body with { Id = id };
            await sender.Send(cmd);
            return Results.NoContent();
        });

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender) =>
        {
            await sender.Send(new DeleteApartment.Command(id));
            return Results.NoContent();
        });

        return app;
    }
}


