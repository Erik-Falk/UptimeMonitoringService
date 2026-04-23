namespace UptimeMonitor.Services;

public class StartupMonitorImporter
{
    private readonly UptimeRobotService _uptimeRobotService;
    private readonly TenantStore _tenantStore;
    private readonly ILogger<StartupMonitorImporter> _logger;

    public StartupMonitorImporter(
        UptimeRobotService uptimeRobotService,
        TenantStore tenantStore,
        ILogger<StartupMonitorImporter> logger)
    {
        _uptimeRobotService = uptimeRobotService;
        _tenantStore = tenantStore;
        _logger = logger;
    }
    public async Task ImportAsync()
    {
        var result = await _uptimeRobotService.GetAllMonitorsAsync();

        if (!result.Success)
        {
            _logger.LogWarning(
                "Kunde inte importera monitorer från UptimeRobot vid uppstart: {Error}",
                result.ErrorMessage);
            return;
        }

        _tenantStore.ReplaceAll(result.Monitors);

        _logger.LogInformation(
            "Importerade {Count} monitorer från UptimeRobot vid uppstart.",
            result.Monitors.Count);
    }
}
