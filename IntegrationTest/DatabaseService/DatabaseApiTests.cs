using System.Net;
using System.Net.Http.Json;
using DatabaseService.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Models;

namespace IntegrationTest.DatabaseService;

public class DatabaseApiTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;
    
    public DatabaseApiTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Post_CreatesEntry_WhenEntryValid()
    {
        // arrange
        var idToVerify = Guid.NewGuid().ToString();
        var dto = new TrafficLightStatusDto
        {
            Color = "green",
            LastChanged = DateTime.MinValue,
            TrafficLightId = idToVerify
        };

        // act
        var response = await _client.PostAsJsonAsync("traffic-light-status", dto);

        // assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetService<TrafficLightContext>()!;
        var inDb = await context.TrafficLight
            .FirstOrDefaultAsync(tl => tl.TrafficLightId == idToVerify);
        Assert.NotNull(inDb);
    }
}