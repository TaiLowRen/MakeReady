using MakeReady.Data;
using MakeReady.Data.CompiledModels;
using MakeReady.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MakeReady;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "makeready.db");
        var dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .UseModel(AppDbContextModel.Instance)
            .Options;
        builder.Services.AddSingleton(dbOptions);
        builder.Services.AddSingleton<IDbContextFactory<AppDbContext>, DatabaseInitializer>();

        builder.Services.AddScoped<FirearmService>();
        builder.Services.AddScoped<HitFactorService>();
        builder.Services.AddScoped<MaintenanceService>();
        builder.Services.AddScoped<ExportService>();
        builder.Services.AddScoped<AmmoService>();
        builder.Services.AddSingleton<AlertService>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        // Kick off the (deferred, background) database initialization now so it
        // starts as soon as possible, without blocking app launch on it.
        _ = app.Services.GetRequiredService<IDbContextFactory<AppDbContext>>();

        return app;
    }
}
