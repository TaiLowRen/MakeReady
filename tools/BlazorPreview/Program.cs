using MakeReady.Data;
using MakeReady.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var dbPath = Path.Combine(AppContext.BaseDirectory, "makeready-preview.db");
var dbOptions = new DbContextOptionsBuilder<AppDbContext>()
    .UseSqlite($"Data Source={dbPath}")
    .Options;
builder.Services.AddSingleton(dbOptions);
builder.Services.AddSingleton<IDbContextFactory<AppDbContext>, DatabaseInitializer>();

builder.Services.AddScoped<FirearmService>();
builder.Services.AddScoped<HitFactorService>();
builder.Services.AddScoped<MaintenanceService>();
builder.Services.AddScoped<ExportService>();
builder.Services.AddScoped<AmmoService>();
builder.Services.AddSingleton<AlertService>();

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<MakeReady.App>()
    .AddInteractiveServerRenderMode();

app.Run();
