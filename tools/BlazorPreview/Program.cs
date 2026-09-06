using MakeReady.Data;
using MakeReady.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var dbPath = Path.Combine(AppContext.BaseDirectory, "makeready-preview.db");
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

builder.Services.AddScoped<FirearmService>();
builder.Services.AddScoped<HitFactorService>();
builder.Services.AddScoped<MaintenanceService>();
builder.Services.AddScoped<ExportService>();
builder.Services.AddScoped<AmmoService>();
builder.Services.AddSingleton<AlertService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    using var db = dbFactory.CreateDbContext();
    db.Database.EnsureCreated();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<MakeReady.App>()
    .AddInteractiveServerRenderMode();

app.Run();
