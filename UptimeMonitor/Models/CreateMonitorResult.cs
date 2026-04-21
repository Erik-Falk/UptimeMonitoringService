namespace UptimeMonitor.Models;

public class CreateMonitorResult
{
    public bool Success { get; set; }
    public string? MonitorId { get; set; }
    public string? ErrorMessage { get; set; }
}