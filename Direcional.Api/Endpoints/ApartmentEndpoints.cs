using Direcional.Application.Apartments;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace Direcional.Api.Endpoints;

public static class ApartmentEndpoints
{
    public record ApartmentCreateRequest(string Code, string Block, int Floor, int Number, decimal Price);
    public record ApartmentUpdateRequest(string Code, string Block, int Floor, int Number, decimal Price);

    public static IEndpointRouteBuilder MapApartments(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/apartments").RequireAuthorization();

        group.MapGet("/", async (int page, int pageSize, string? search, ISender sender) =>
        {
            var result = await sender.Send(new GetApartments.Query(page, pageSize, search));
            return Results.Ok(result);
        })
        .WithTags("Apartments")
        .WithOpenApi();

        group.MapPost("/", async (ApartmentCreateRequest body, ISender sender) =>
        {
            var result = await sender.Send(new CreateApartment.CreateApartmentCommand(body.Code, body.Block, body.Floor, body.Number, body.Price));
            return Results.Created($"/apartments/{result.Id}", result);
        })
        .WithTags("Apartments")
        .WithOpenApi();

        group.MapPut("/{id:guid}", async (Guid id, ApartmentUpdateRequest body, ISender sender) =>
        {
            await sender.Send(new UpdateApartment.UpdateApartmentCommand(id, body.Code, body.Block, body.Floor, body.Number, body.Price));
            return Results.NoContent();
        })
        .WithTags("Apartments")
        .WithOpenApi();

        group.MapDelete("/{id:guid}", async (Guid id, ISender sender) =>
        {
            await sender.Send(new DeleteApartment.DeleteApartmentCommand(id));
            return Results.NoContent();
        })
        .WithTags("Apartments")
        .WithOpenApi();

        return app;
    }
}


