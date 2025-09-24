using System;
using System.Threading;
using System.Threading.Tasks;
using Direcional.Application.Abstractions;
using Direcional.Application.Reservations;
using Direcional.Application.Sales;
using Direcional.Domain.Entities;
using Direcional.Domain.Enums;
using Direcional.Infrastructure.Persistence;
using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Direcional.Tests;

public class ReservationAndSaleTests
{
    private static AppDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task CreateReservation_Publishes_Event_And_Reserves_Apartment()
    {
        using var db = CreateInMemoryDb();
        var client = new Client { Id = Guid.NewGuid(), Name = "Test", Email = "t@e.com", Document = "111", Phone = "" };
        var apartment = new Apartment { Id = Guid.NewGuid(), Code = "Z-999", Block = "Z", Floor = 9, Number = 999, Price = 100000m, Status = ApartmentStatus.Available };
        db.Clients.Add(client);
        db.Apartments.Add(apartment);
        await db.SaveChangesAsync();

        var publisher = Substitute.For<IPublishEndpoint>();
        IAppDbContext appDb = db;
        var handler = new CreateReservation.Handler(appDb, publisher);

        var cmd = new CreateReservation.CreateReservationCommand(client.Id, apartment.Id, 24);
        var result = await handler.Handle(cmd, CancellationToken.None);

        result.Id.Should().NotBeEmpty();

        var updatedApt = await db.Apartments.FirstAsync(a => a.Id == apartment.Id);
        updatedApt.Status.Should().Be(ApartmentStatus.Reserved);

        await publisher.Received(1).Publish(Arg.Any<ReservationCreated>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConfirmSale_Publishes_Event_And_Sets_Apartment_Sold()
    {
        using var db = CreateInMemoryDb();
        var client = new Client { Id = Guid.NewGuid(), Name = "Buyer", Email = "b@e.com", Document = "222", Phone = "" };
        var apartment = new Apartment { Id = Guid.NewGuid(), Code = "Y-888", Block = "Y", Floor = 8, Number = 888, Price = 200000m, Status = ApartmentStatus.Available };
        db.Clients.Add(client);
        db.Apartments.Add(apartment);
        await db.SaveChangesAsync();

        var publisher = Substitute.For<IPublishEndpoint>();
        IAppDbContext appDb = db;
        var handler = new ConfirmSale.Handler(appDb, publisher);

        var cmd = new ConfirmSale.ConfirmSaleCommand(client.Id, apartment.Id, null, 10000m, 200000m);
        var result = await handler.Handle(cmd, CancellationToken.None);

        result.Id.Should().NotBeEmpty();

        var updatedApt = await db.Apartments.FirstAsync(a => a.Id == apartment.Id);
        updatedApt.Status.Should().Be(ApartmentStatus.Sold);

        await publisher.Received(1).Publish(Arg.Any<SaleConfirmed>(), Arg.Any<CancellationToken>());
    }
}


