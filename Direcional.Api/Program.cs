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
using Microsoft.OpenApi.Models;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Direcional API", Version = "v1" });

    // Avoid schema ID collisions for record types like Command/Result across features
    c.CustomSchemaIds(t =>
    {
        var full = t.FullName ?? t.Name;
        return full.Replace('.', '_').Replace('+', '_');
    });

    // Exclude OPTIONS methods from Swagger documentation (safe null check)
    c.DocInclusionPredicate((docName, apiDesc) =>
        !string.Equals(apiDesc.HttpMethod, "OPTIONS", StringComparison.OrdinalIgnoreCase));

    // Security scheme
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter 'Bearer {token}'",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };

    c.AddSecurityDefinition("Bearer", securityScheme);

    // Correctly reference scheme in requirement
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

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

// Simple CORS for Docker environment
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Disable automatic OPTIONS endpoint creation
builder.Services.Configure<Microsoft.AspNetCore.Routing.RouteOptions>(options =>
{
    options.SuppressCheckForUnhandledSecurityMetadata = true;
});

builder.Services.AddMediatR(typeof(Direcional.Application.Abstractions.IAppDbContext).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(Direcional.Application.Abstractions.IAppDbContext).Assembly);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Direcional API v1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "Direcional API";
    });
}

app.UseSerilogRequestLogging();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Serve static files if you have wwwroot content (can be removed if not needed)
// Enable static files (Swagger UI assets are included by Swashbuckle's embedded file provider)
app.UseStaticFiles();

// Auto-apply EF Core migrations at startup (skip in Testing)
if (!app.Environment.IsEnvironment("Testing"))
{
    try
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
        Seed.SeedAdminUser(db);
        Seed.SeedApartments(db);
        Seed.SeedDefaultClient(db);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Database migration/seed failed at startup");
    }
}

app.MapGet("/", () => Results.Ok(new { name = "Direcional API", status = "running" }));

// CORS will handle OPTIONS requests automatically

Direcional.Api.Endpoints.AuthEndpoints.MapAuth(app);
Direcional.Api.Endpoints.ClientEndpoints.MapClients(app);
Direcional.Api.Endpoints.ApartmentEndpoints.MapApartments(app);
Direcional.Api.Endpoints.ReservationEndpoints.MapReservations(app);
Direcional.Api.Endpoints.SaleEndpoints.MapSales(app);

// Dev-only: trigger data seed (requires auth)
app.MapPost("/dev/seed", (AppDbContext db) =>
{
    Seed.SeedApartments(db);
    Seed.SeedDefaultClient(db);
    return Results.Ok(new { seeded = true });
}).RequireAuthorization().WithTags("Dev").WithOpenApi();

app.Run();

public partial class Program { }
