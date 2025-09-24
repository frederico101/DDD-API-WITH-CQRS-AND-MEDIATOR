using Direcional.Application.Reservations;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace Direcional.Api.Endpoints;

public static class ReservationEndpoints
{
    public static IEndpointRouteBuilder MapReservations(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/reservations").RequireAuthorization();

        group.MapPost("/", async (CreateReservation.Command cmd, ISender sender) =>
        {
            var result = await sender.Send(cmd);
            return Results.Created($"/reservations/{result.Id}", result);
        });

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender) =>
        {
            await sender.Send(new CancelReservation.Command(id));
            return Results.NoContent();
        });

        return app;
    }
}


