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
        })
        .WithTags("Clients")
        .WithOpenApi();

        group.MapPost("/", async (CreateClient.CreateClientCommand cmd, ISender sender) =>
        {
            var result = await sender.Send(cmd);
            return Results.Created($"/clients/{result.Id}", result);
        })
        .WithTags("Clients")
        .WithOpenApi();

        group.MapPut("/{id:guid}", async (Guid id, UpdateClient.UpdateClientCommand body, ISender sender) =>
        {
            var cmd = body with { Id = id };
            await sender.Send(cmd);
            return Results.NoContent();
        })
        .WithTags("Clients")
        .WithOpenApi();

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender) =>
        {
            await sender.Send(new DeleteClient.DeleteClientCommand(id));
            return Results.NoContent();
        })
        .WithTags("Clients")
        .WithOpenApi();

        return app;
    }
}


