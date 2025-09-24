using System.Threading.Tasks;
using Direcional.Application.Reservations;
using Direcional.Application.Sales;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Direcional.Infrastructure.Messaging;

public class ReservationCreatedConsumer : IConsumer<ReservationCreated>
{
    private readonly ILogger<ReservationCreatedConsumer> _logger;
    public ReservationCreatedConsumer(ILogger<ReservationCreatedConsumer> logger) => _logger = logger;

    public Task Consume(ConsumeContext<ReservationCreated> context)
    {
        var e = context.Message;
        _logger.LogInformation("[ReservationCreated] reservationId={ReservationId} clientId={ClientId} apartmentId={ApartmentId} expiresAt={Expires}", e.ReservationId, e.ClientId, e.ApartmentId, e.ExpiresAtUtc);
        return Task.CompletedTask;
    }
}

public class SaleConfirmedConsumer : IConsumer<SaleConfirmed>
{
    private readonly ILogger<SaleConfirmedConsumer> _logger;
    public SaleConfirmedConsumer(ILogger<SaleConfirmedConsumer> logger) => _logger = logger;

    public Task Consume(ConsumeContext<SaleConfirmed> context)
    {
        var e = context.Message;
        _logger.LogInformation("[SaleConfirmed] saleId={SaleId} clientId={ClientId} apartmentId={ApartmentId} total={Total} down={Down}", e.SaleId, e.ClientId, e.ApartmentId, e.TotalPrice, e.DownPayment);
        return Task.CompletedTask;
    }
}


