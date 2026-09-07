using MakeReady.Data;
using MakeReady.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var dataPath = Path.Combine(AppContext.BaseDirectory, "makeready-preview-data.json");
builder.Services.AddSingleton(new JsonDataStore(dataPath));

builder.Services.AddScoped<FirearmService>();
builder.Services.AddScoped<HitFactorService>();
builder.Services.AddScoped<MaintenanceService>();
builder.Services.AddScoped<ExportService>();
builder.Services.AddScoped<AmmoService>();
builder.Services.AddSingleton<AlertService>();
builder.Services.AddSingleton<NavDrawerState>();

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<MakeReady.App>()
    .AddInteractiveServerRenderMode();

app.Run();
