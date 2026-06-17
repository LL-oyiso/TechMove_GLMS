using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using GLMS.Shared.Dtos;
using GLMS.Shared.Enums;

namespace Glms_Monolith_Test.Integration;

public class ApiIntegrationTests : IClassFixture<GlmsApiFactory>
{
    private readonly GlmsApiFactory _factory;

    public ApiIntegrationTests(GlmsApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetContracts_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/contracts");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest { Username = "admin", Password = "Admin123!" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        auth.Should().NotBeNull();
        auth!.Token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest { Username = "admin", Password = "wrong-password" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetClients_WithToken_Returns200AndNonNull()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/clients");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var clients = await response.Content.ReadFromJsonAsync<List<ClientDto>>();
        clients.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateThenGetClient_PersistsData()
    {
        var client = await CreateAuthenticatedClientAsync();

        // Create
        var input = new ClientInputDto { Name = "Integration Co", ContactDetails = "it@test.com", Region = "Gauteng" };
        var createResponse = await client.PostAsJsonAsync("/api/clients", input);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<ClientDto>();
        created.Should().NotBeNull();
        created!.Id.Should().BeGreaterThan(0);

        // Read back (data integrity)
        var getResponse = await client.GetAsync($"/api/clients/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await getResponse.Content.ReadFromJsonAsync<ClientDto>();
        fetched.Should().NotBeNull();
        fetched!.Name.Should().Be("Integration Co");
        fetched.ContactDetails.Should().Be("it@test.com");
        fetched.Region.Should().Be("Gauteng");
    }

    [Fact]
    public async Task CreateThenGetContract_PersistsDataAndLinksClient()
    {
        var client = await CreateAuthenticatedClientAsync();

        // Arrange: a client to attach the contract to.
        var clientResponse = await client.PostAsJsonAsync("/api/clients",
            new ClientInputDto { Name = "Contract Owner", ContactDetails = "owner@test.com", Region = "KZN" });
        var owner = await clientResponse.Content.ReadFromJsonAsync<ClientDto>();

        // Create contract
        var contractInput = new ContractInputDto
        {
            ClientId = owner!.Id,
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddMonths(6),
            Status = ContractStatus.Draft,
            ServiceLevel = "Gold"
        };
        var createResponse = await client.PostAsJsonAsync("/api/contracts", contractInput);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResponse.Content.ReadFromJsonAsync<ContractDto>();
        created.Should().NotBeNull();

        // Read back and verify the data + client link survived the round trip.
        var getResponse = await client.GetAsync($"/api/contracts/{created!.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var fetched = await getResponse.Content.ReadFromJsonAsync<ContractDto>();
        fetched.Should().NotBeNull();
        fetched!.ServiceLevel.Should().Be("Gold");
        fetched.Status.Should().Be(ContractStatus.Draft);
        fetched.ClientId.Should().Be(owner.Id);
        fetched.ClientName.Should().Be("Contract Owner");
    }

    [Fact]
    public async Task CreateContract_WithInvalidServiceLevel_Returns400()
    {
        var client = await CreateAuthenticatedClientAsync();

        var ownerResponse = await client.PostAsJsonAsync("/api/clients",
            new ClientInputDto { Name = "Bad Level Co", ContactDetails = "b@test.com", Region = "WC" });
        var owner = await ownerResponse.Content.ReadFromJsonAsync<ClientDto>();

        var contractInput = new ContractInputDto
        {
            ClientId = owner!.Id,
            StartDate = DateTime.UtcNow.Date,
            EndDate = DateTime.UtcNow.Date.AddMonths(1),
            Status = ContractStatus.Draft,
            ServiceLevel = "NotARealLevel"
        };

        var response = await client.PostAsJsonAsync("/api/contracts", contractInput);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();
        var loginResponse = await client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest { Username = "admin", Password = "Admin123!" });
        loginResponse.EnsureSuccessStatusCode();

        var auth = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);
        return client;
    }
}
