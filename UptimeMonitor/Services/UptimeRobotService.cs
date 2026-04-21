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

    public async Task<MonitorStatusResult> GetMonitorAsync(string monitorId)
    {
        var apiKey = _configuration["UptimeRobot:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return new MonitorStatusResult
            {
                Success = false,
                ErrorMessage = "UptimeRobot API key saknas."
            };
        }

        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("api_key", apiKey),
            new KeyValuePair<string, string>("format", "json"),
            new KeyValuePair<string, string>("monitors", monitorId),
            new KeyValuePair<string, string>("custom_uptime_ratios", "30")
        });

        var response = await _httpClient.PostAsync("https://api.uptimerobot.com/v2/getMonitors", content);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return new MonitorStatusResult
            {
                Success = false,
                ErrorMessage = $"HTTP {(int)response.StatusCode}: {responseBody}"
            };
        }

        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            if (!root.TryGetProperty("stat", out var stat) || stat.GetString() != "ok")
            {
                return new MonitorStatusResult
                {
                    Success = false,
                    ErrorMessage = responseBody
                };
            }

            var monitors = root.GetProperty("monitors");

            if (monitors.GetArrayLength() == 0)
            {
                return new MonitorStatusResult
                {
                    Success = false,
                    ErrorMessage = "Ingen monitor hittades."
                };
            }

            var monitor = monitors[0];

            var statusCode = monitor.GetProperty("status").GetInt32();
            var statusText = MapStatus(statusCode);

            double? uptime = null;

            if (monitor.TryGetProperty("custom_uptime_ratio", out var uptimeProp))
            {
                var uptimeString = uptimeProp.GetString();

                if (double.TryParse(
                        uptimeString,
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out var parsedUptime))
                {
                    uptime = parsedUptime;
                }
            }

            return new MonitorStatusResult
            {
                Success = true,
                Status = statusText,
                Uptime = uptime
            };
        }
        catch
        {
            return new MonitorStatusResult
            {
                Success = false,
                ErrorMessage = "Kunde inte tolka monitor-svaret från UptimeRobot."
            };
        }
    }

    private static string MapStatus(int statusCode)
    {
        return statusCode switch
        {
            0 => "Paused",
            1 => "Not checked yet",
            2 => "Up",
            8 => "Seems down",
            9 => "Down",
            _ => "Unknown"
        };
    }
}