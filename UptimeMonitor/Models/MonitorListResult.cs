namespace UptimeMonitor.Models;

public class MonitorListResult
{
    public bool Success { get; set; }
    public List<Tenant> Monitors { get; set; } = new();
    public string? ErrorMessage { get; set; }
}
