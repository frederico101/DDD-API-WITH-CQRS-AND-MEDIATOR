using System.Text;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Direcional.Infrastructure;
using Direcional.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;
using Direcional.Domain.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddProblemDetails();
builder.Services.AddMapster();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwt = builder.Configuration.GetSection("Jwt");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!))
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddMediatR(typeof(Direcional.Application.Abstractions.IAppDbContext).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(Direcional.Application.Abstractions.IAppDbContext).Assembly);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();

// Auto-apply EF Core migrations at startup (dev/test convenience)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    Seed.SeedAdminUser(db);
}

app.MapGet("/", () => Results.Ok(new { name = "Direcional API", status = "running" }));
Direcional.Api.Endpoints.AuthEndpoints.MapAuth(app);
Direcional.Api.Endpoints.ClientEndpoints.MapClients(app);

app.Run();
