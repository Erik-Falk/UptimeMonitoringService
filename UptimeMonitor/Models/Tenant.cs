using System.ComponentModel.DataAnnotations;

namespace UptimeMonitor.Models
{
    public class Tenant
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Namn måste anges")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "URL måste anges")]
        [Url(ErrorMessage = "Ange en giltig URL")]
        public string Url { get; set; } = string.Empty;

        [Range(0, 100, ErrorMessage = "SLA måste vara mellan 0 och 100")]
        public double SlaTarget { get; set; } = 99.9;

        public string? UptimeRobotId { get; set; }
        public string Status { get; set; } = "Unknown";
        public double? CurrentUptime { get; set; }
    }
}