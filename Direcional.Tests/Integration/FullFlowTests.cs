using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Direcional.Tests.Integration;

public class FullFlowTests : IClassFixture<TestWebAppFactory>
{
    private readonly TestWebAppFactory _factory;
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    public FullFlowTests(TestWebAppFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_CreateApartment_CreateReservation_Succeeds()
    {
        var client = _factory.CreateClient();

        var loginResp = await client.PostAsJsonAsync("/auth/login", new { username = "admin", password = "admin123" });
        loginResp.EnsureSuccessStatusCode();
        var loginJson = await loginResp.Content.ReadFromJsonAsync<LoginResponse>(_json);
        loginJson!.AccessToken.Should().NotBeNullOrWhiteSpace();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginJson.AccessToken);

        var aptResp = await client.PostAsJsonAsync("/apartments", new { code = "IT-100", block = "IT", floor = 10, number = 100, price = 123456.78M });
        aptResp.EnsureSuccessStatusCode();
        var aptJson = await aptResp.Content.ReadFromJsonAsync<IdResponse>(_json);

        var resResp = await client.PostAsJsonAsync("/reservations", new { clientId = await GetAnyClientId(client), apartmentId = aptJson!.Id, expiresHours = 24 });
        resResp.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Login_CreateApartment_ConfirmSale_Succeeds()
    {
        var client = _factory.CreateClient();

        var loginResp = await client.PostAsJsonAsync("/auth/login", new { username = "admin", password = "admin123" });
        loginResp.EnsureSuccessStatusCode();
        var loginJson = await loginResp.Content.ReadFromJsonAsync<LoginResponse>(_json);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginJson!.AccessToken);

        var aptResp = await client.PostAsJsonAsync("/apartments", new { code = "IT-200", block = "IT", floor = 20, number = 200, price = 222222.22M });
        aptResp.EnsureSuccessStatusCode();
        var aptJson = await aptResp.Content.ReadFromJsonAsync<IdResponse>(_json);

        var clientId = await GetAnyClientId(client);
        var saleResp = await client.PostAsJsonAsync("/sales", new { clientId, apartmentId = aptJson!.Id, reservationId = (Guid?)null, downPayment = 10000M, totalPrice = 222222.22M });
        saleResp.EnsureSuccessStatusCode();
    }

    private static async Task<Guid> GetAnyClientId(HttpClient client)
    {
        var resp = await client.GetAsync("/clients?page=1&pageSize=1");
        resp.EnsureSuccessStatusCode();
        var list = await resp.Content.ReadFromJsonAsync<ClientListResponse>();
        return list!.Items[0].Id;
    }

    private sealed record LoginResponse(string AccessToken);
    private sealed record IdResponse(Guid Id);
    private sealed record ClientListResponse(int Total, ClientItem[] Items);
    private sealed record ClientItem(Guid Id, string Name, string Email, string Document);
}


