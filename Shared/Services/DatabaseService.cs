using System.Text;
using System.Text.Json;
using Shared.Models;

namespace Shared.Services;

public class DatabaseService : IDatabaseService
{
    private readonly HttpClient _httpClient;

    public DatabaseService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://database-service:8080");
    }

    public async Task AddTrafficLightStatus(TrafficLightStatusDto statusDto)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "traffic-light-status");
        message.Content = new StringContent(JsonSerializer.Serialize(statusDto), Encoding.UTF8, "application/json");
        await _httpClient.SendAsync(message);
    }
}