using UptimeMonitor.Models;

namespace UptimeMonitor.Services;

public class TenantStore
{
    private readonly List<Tenant> _tenants = new();
    private readonly Lock _lock = new();

    public List<Tenant> GetAll()
    {
        lock (_lock)
        {
            return _tenants.Select(Clone).ToList();
        }
    }

    public void ReplaceAll(IEnumerable<Tenant> tenants)
    {
        lock (_lock)
        {
            _tenants.Clear();
            _tenants.AddRange(tenants.Select(Clone));
        }
    }

    public void Add(Tenant tenant)
    {
        lock (_lock)
        {
            _tenants.Add(Clone(tenant));
        }
    }

    public void UpdateStatus(string uptimeRobotId, string status, double? currentUptime)
    {
        lock (_lock)
        {
            var tenant = _tenants.FirstOrDefault(t => t.UptimeRobotId == uptimeRobotId);

            if (tenant is null)
                return;

            tenant.Status = status;
            tenant.CurrentUptime = currentUptime;
        }
    }

    public void UpdateMonitorDetails(Tenant monitor)
    {
        if (string.IsNullOrWhiteSpace(monitor.UptimeRobotId))
            return;

        lock (_lock)
        {
            var tenant = _tenants.FirstOrDefault(t => t.UptimeRobotId == monitor.UptimeRobotId);

            if (tenant is null)
                return;

            tenant.Name = monitor.Name;
            tenant.Url = monitor.Url;
            tenant.Status = monitor.Status;
            tenant.CurrentUptime = monitor.CurrentUptime;
        }
    }

    public int GetNextId()
    {
        lock (_lock)
        {
            return _tenants.Count == 0 ? 1 : _tenants.Max(t => t.Id) + 1;
        }
    }

    private static Tenant Clone(Tenant tenant)
    {
        return new Tenant
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Url = tenant.Url,
            SlaTarget = tenant.SlaTarget,
            UptimeRobotId = tenant.UptimeRobotId,
            Status = tenant.Status,
            CurrentUptime = tenant.CurrentUptime
        };
    }
}
