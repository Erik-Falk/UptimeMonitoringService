using Microsoft.AspNetCore.Mvc;
using UptimeMonitor.Models;
using UptimeMonitor.Services;

namespace UptimeMonitor.Controllers;

public class TenantsController : Controller
{
    private readonly UptimeRobotService _uptimeService;
    private static readonly List<Tenant> _tenants = new();

    public TenantsController(UptimeRobotService uptimeService)
    {
        _uptimeService = uptimeService;
    }

    public async Task<IActionResult> Index()
    {
        foreach (var tenant in _tenants)
        {
            if (string.IsNullOrWhiteSpace(tenant.UptimeRobotId))
                continue;

            var monitorResult = await _uptimeService.GetMonitorAsync(tenant.UptimeRobotId);

            if (monitorResult.Success)
            {
                tenant.Status = monitorResult.Status;
                tenant.CurrentUptime = monitorResult.Uptime;
            }
            else
            {
                tenant.Status = "Error";
            }
        }

        return View(_tenants);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Tenant tenant)
    {
        if (!ModelState.IsValid)
            return View(tenant);

        var result = await _uptimeService.CreateMonitorAsync(tenant.Name, tenant.Url);

        if (!result.Success)
        {
            ModelState.AddModelError("", result.ErrorMessage ?? "Kunde inte skapa monitor i UptimeRobot.");
            return View(tenant);
        }

        tenant.UptimeRobotId = result.MonitorId;
        tenant.Status = "Created";
        tenant.CurrentUptime = null;
        tenant.Id = _tenants.Count == 0 ? 1 : _tenants.Max(t => t.Id) + 1;

        _tenants.Add(tenant);

        return RedirectToAction(nameof(Index));
    }
}