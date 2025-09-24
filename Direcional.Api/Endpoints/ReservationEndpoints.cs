using Direcional.Application.Reservations;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace Direcional.Api.Endpoints;

public static class ReservationEndpoints
{
    public static IEndpointRouteBuilder MapReservations(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/reservations").RequireAuthorization();

        group.MapGet("/", async (int page, int pageSize, ISender sender) =>
        {
            var result = await sender.Send(new GetReservations.Query(page, pageSize));
            return Results.Ok(result);
        })
        .WithTags("Reservations")
        .WithOpenApi();

        group.MapPost("/", async (CreateReservation.CreateReservationCommand cmd, ISender sender) =>
        {
            var result = await sender.Send(cmd);
            return Results.Created($"/reservations/{result.Id}", result);
        })
        .WithTags("Reservations")
        .WithOpenApi();

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender) =>
        {
            await sender.Send(new CancelReservation.CancelReservationCommand(id));
            return Results.NoContent();
        })
        .WithTags("Reservations")
        .WithOpenApi();

        return app;
    }
}


