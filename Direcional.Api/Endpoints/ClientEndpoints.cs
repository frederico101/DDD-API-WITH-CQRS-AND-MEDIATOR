using Direcional.Application.Clients;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace Direcional.Api.Endpoints;

public static class ClientEndpoints
{
    public static IEndpointRouteBuilder MapClients(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/clients").RequireAuthorization();

        group.MapGet("/", async (int page, int pageSize, string? search, ISender sender) =>
        {
            var result = await sender.Send(new GetClients.Query(page, pageSize, search));
            return Results.Ok(result);
        });

        group.MapPost("/", async (CreateClient.Command cmd, ISender sender) =>
        {
            var result = await sender.Send(cmd);
            return Results.Created($"/clients/{result.Id}", result);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateClient.Command body, ISender sender) =>
        {
            var cmd = body with { Id = id };
            await sender.Send(cmd);
            return Results.NoContent();
        });

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender) =>
        {
            await sender.Send(new DeleteClient.Command(id));
            return Results.NoContent();
        });

        return app;
    }
}


