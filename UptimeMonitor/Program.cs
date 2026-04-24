using UptimeMonitor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<TenantStore>();
builder.Services.AddHttpClient<UptimeRobotService>();
builder.Services.AddScoped<StartupMonitorImporter>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Tenants}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var importer = scope.ServiceProvider.GetRequiredService<StartupMonitorImporter>();
    await importer.ImportAsync();
}

app.Run();
