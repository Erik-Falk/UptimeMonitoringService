using Microsoft.AspNetCore.Mvc;
using UptimeMonitor.Models;
using UptimeMonitor.Services;

namespace UptimeMonitor.Controllers;

public class TenantsController : Controller
{
    private readonly UptimeRobotService _uptimeService;
    private readonly TenantStore _tenantStore;

    public TenantsController(UptimeRobotService uptimeService, TenantStore tenantStore)
    {
        _uptimeService = uptimeService;
        _tenantStore = tenantStore;
    }

    public async Task<IActionResult> Index()
    {
        var tenants = _tenantStore.GetAll();
        var monitorResult = await _uptimeService.GetAllMonitorsAsync();

        if (!monitorResult.Success)
        {
            ViewBag.ApiWarning = monitorResult.ErrorMessage;
            return View(tenants);
        }

        var monitorLookup = monitorResult.Monitors
            .Where(m => !string.IsNullOrWhiteSpace(m.UptimeRobotId))
            .ToDictionary(m => m.UptimeRobotId!, m => m);

        foreach (var tenant in tenants)
        {
            if (string.IsNullOrWhiteSpace(tenant.UptimeRobotId))
                continue;

            if (!monitorLookup.TryGetValue(tenant.UptimeRobotId, out var monitor))
                continue;

            tenant.Name = monitor.Name;
            tenant.Url = monitor.Url;
            tenant.Status = monitor.Status;
            tenant.CurrentUptime = monitor.CurrentUptime;
            _tenantStore.UpdateMonitorDetails(tenant);
        }

        return View(tenants);
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
        tenant.Id = _tenantStore.GetNextId();

        _tenantStore.Add(tenant);

        return RedirectToAction(nameof(Index));
    }
}