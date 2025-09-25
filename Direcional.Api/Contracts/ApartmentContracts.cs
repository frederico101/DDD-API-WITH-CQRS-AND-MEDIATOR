namespace Direcional.Api.Contracts;

public record ApartmentCreateRequest(string Code, string Block, int Floor, int Number, decimal Price);
public record ApartmentUpdateRequest(string Code, string Block, int Floor, int Number, decimal Price);
