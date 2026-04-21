using System.Text.Json;
using UptimeMonitor.Models;

namespace UptimeMonitor.Services;

public class UptimeRobotService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public UptimeRobotService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<CreateMonitorResult> CreateMonitorAsync(string name, string url)
    {
        var apiKey = _configuration["UptimeRobot:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return new CreateMonitorResult
            {
                Success = false,
                ErrorMessage = "UptimeRobot API key saknas i konfigurationen."
            };
        }

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("api_key", apiKey),
            new KeyValuePair<string, string>("format", "json"),
            new KeyValuePair<string, string>("type", "1"),
            new KeyValuePair<string, string>("friendly_name", name),
            new KeyValuePair<string, string>("url", url)
        });

        var response = await _httpClient.PostAsync("https://api.uptimerobot.com/v2/newMonitor", content);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return new CreateMonitorResult
            {
                Success = false,
                ErrorMessage = $"HTTP {(int)response.StatusCode}: {responseBody}"
            };
        }

        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            if (root.TryGetProperty("stat", out var stat) && stat.GetString() == "ok")
            {
                var monitor = root.GetProperty("monitor");
                var monitorId = monitor.GetProperty("id").ToString();

                return new CreateMonitorResult
                {
                    Success = true,
                    MonitorId = monitorId
                };
            }

            return new CreateMonitorResult
            {
                Success = false,
                ErrorMessage = responseBody
            };
        }
        catch
        {
            return new CreateMonitorResult
            {
                Success = false,
                ErrorMessage = "Kunde inte tolka svaret från UptimeRobot."
            };
        }
    }
}