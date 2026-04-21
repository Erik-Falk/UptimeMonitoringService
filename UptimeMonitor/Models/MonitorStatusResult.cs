namespace UptimeMonitor.Models
{
    public class MonitorStatusResult
    {
        public bool Success { get; set; }
        public string Status { get; set; } = "Unknown";
        public double? Uptime { get; set; }
        public string? ErrorMessage { get; set; }
    }
}