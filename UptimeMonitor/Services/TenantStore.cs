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
            return _tenants.Select(t => t.Clone()).ToList();
        }
    }

    public void ReplaceAll(IEnumerable<Tenant> tenants)
    {
        lock (_lock)
        {
            _tenants.Clear();
            _tenants.AddRange(tenants.Select(t => t.Clone()));
        }
    }

    public void Add(Tenant tenant)
    {
        lock (_lock)
        {
            _tenants.Add(tenant.Clone());
        }
    }

    public void UpdateStatus(string uptimeRobotId, string status, double? currentUptime)
    {
        UpdateTenant(uptimeRobotId, tenant =>
        {
            tenant.Status = status;
            tenant.CurrentUptime = currentUptime;
        });
    }

    public void UpdateMonitorDetails(Tenant monitor)
    {
        if (string.IsNullOrWhiteSpace(monitor.UptimeRobotId))
            return;

        UpdateTenant(monitor.UptimeRobotId, tenant =>
        {
            tenant.Name = monitor.Name;
            tenant.Url = monitor.Url;
            tenant.Status = monitor.Status;
            tenant.CurrentUptime = monitor.CurrentUptime;
        });
    }

    private void UpdateTenant(string? uptimeRobotId, Action<Tenant> updateAction)
    {
        if (string.IsNullOrWhiteSpace(uptimeRobotId))
            return;

        lock (_lock)
        {
            var tenant = _tenants.FirstOrDefault(t => t.UptimeRobotId == uptimeRobotId);
            if (tenant is not null)
            {
                updateAction(tenant);
            }
        }
    }

    public int GetNextId()
    {
        lock (_lock)
        {
            return _tenants.Count == 0 ? 1 : _tenants.Max(t => t.Id) + 1;
        }
    }
}
