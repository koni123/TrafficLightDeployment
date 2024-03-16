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
        var baseAddress = Environment.GetEnvironmentVariable("DB_BASE_URL") ?? "http://localhost:8080";
        _httpClient.BaseAddress = new Uri(baseAddress);
    }

    public async Task AddTrafficLightStatusAsync(TrafficLightStatusDto statusDto)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "traffic-light-status");
        message.Content = new StringContent(JsonSerializer.Serialize(statusDto), Encoding.UTF8, "application/json");
        await _httpClient.SendAsync(message);
    }

    public async Task<List<TrafficLightStatusDto>> GetCurrentStatusAsync()
    {
        var response = await _httpClient.GetAsync("traffic-light-status");
        var content = await response.Content.ReadAsStringAsync();
        var opt = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<List<TrafficLightStatusDto>>(content, opt) ?? [];
    }

    public async Task AddControlUnitOperationModeAsync(OperationModeDto operationMode)
    {
        using var message = new HttpRequestMessage(HttpMethod.Post, "operation-mode");
        message.Content = new StringContent(JsonSerializer.Serialize(operationMode), Encoding.UTF8, "application/json");
        await _httpClient.SendAsync(message);
    }

    public async Task<OperationModeDto?> GetControlUnitOperationModeAsync()
    {
        var response = await _httpClient.GetAsync("operation-mode");
        var content = await response.Content.ReadAsStringAsync();
        var opt = new JsonSerializerOptions() { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<OperationModeDto>(content, opt) ?? null;
    }

    public async Task<List<TrafficLightStatusDto>> GetAllAsync()
    {
        var response = await _httpClient.GetAsync("traffic-light-status/all");
        return JsonSerializer.Deserialize<List<TrafficLightStatusDto>>(await response.Content.ReadAsStringAsync()) ??
               [];
    }
}