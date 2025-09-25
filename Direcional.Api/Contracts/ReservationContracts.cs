namespace Direcional.Api.Contracts;

public record ReservationCreateRequest(Guid ClientId, Guid ApartmentId);
