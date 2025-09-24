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
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { securityScheme, new string[] { } }
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

// CORS for frontend
const string CorsPolicy = "Frontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, p => p
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());
});

builder.Services.AddMediatR(typeof(Direcional.Application.Abstractions.IAppDbContext).Assembly);
builder.Services.AddValidatorsFromAssembly(typeof(Direcional.Application.Abstractions.IAppDbContext).Assembly);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
// CORS must run before auth so 401 responses include CORS headers
app.UseCors(CorsPolicy);

// Harden preflight handling: respond 200 with CORS headers for any OPTIONS
app.Use(async (context, next) =>
{
    if (string.Equals(context.Request.Method, "OPTIONS", StringComparison.OrdinalIgnoreCase))
    {
        var origin = context.Request.Headers["Origin"].ToString();
        if (!string.IsNullOrEmpty(origin))
        {
            context.Response.Headers["Access-Control-Allow-Origin"] = origin;
            context.Response.Headers["Vary"] = "Origin";
        }
        else
        {
            context.Response.Headers["Access-Control-Allow-Origin"] = "*";
        }
        var reqHeaders = context.Request.Headers["Access-Control-Request-Headers"].ToString();
        var reqMethod = context.Request.Headers["Access-Control-Request-Method"].ToString();
        context.Response.Headers["Access-Control-Allow-Headers"] = string.IsNullOrEmpty(reqHeaders) ? "*" : reqHeaders;
        context.Response.Headers["Access-Control-Allow-Methods"] = string.IsNullOrEmpty(reqMethod) ? "GET,POST,PUT,DELETE,OPTIONS" : reqMethod;
        context.Response.StatusCode = StatusCodes.Status200OK;
        await context.Response.CompleteAsync();
        return;
    }
    await next();
});
app.UseAuthentication();
app.UseAuthorization();

// Auto-apply EF Core migrations at startup (skip in Testing)
if (!app.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
        Seed.SeedAdminUser(db);
        Seed.SeedApartments(db);
        Seed.SeedDefaultClient(db);
    }
}

app.MapGet("/", () => Results.Ok(new { name = "Direcional API", status = "running" }));
Direcional.Api.Endpoints.AuthEndpoints.MapAuth(app);
Direcional.Api.Endpoints.ClientEndpoints.MapClients(app);
Direcional.Api.Endpoints.ApartmentEndpoints.MapApartments(app);
Direcional.Api.Endpoints.ReservationEndpoints.MapReservations(app);
Direcional.Api.Endpoints.SaleEndpoints.MapSales(app);

// Handle CORS preflight (OPTIONS) for all routes
app.MapMethods("/{**any}", new[] { "OPTIONS" }, () => Results.Ok())
   .AllowAnonymous()
   .RequireCors(CorsPolicy);

// Explicit preflight for login to avoid 405 from route matching
app.MapMethods("/auth/login", new[] { "OPTIONS" }, () => Results.Ok())
   .AllowAnonymous()
   .RequireCors(CorsPolicy);

// Dev-only: trigger data seed (requires auth)
app.MapPost("/dev/seed", (AppDbContext db) =>
{
    Seed.SeedApartments(db);
    Seed.SeedDefaultClient(db);
    return Results.Ok(new { seeded = true });
}).RequireAuthorization().WithTags("Dev").WithOpenApi();

app.Run();

public partial class Program { }
