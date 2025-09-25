using Direcional.Application.Auth;
using MediatR;

namespace Direcional.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuth(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth");
        group.MapPost("/login", async (Login.LoginCommand cmd, ISender sender) =>
        {
            var result = await sender.Send(cmd);
            return Results.Ok(result);
        })
        .WithTags("Auth")
        .WithOpenApi();
        return app;
    }
}


