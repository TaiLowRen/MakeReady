using MakeReady.Data;
using MakeReady.Services;
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

        var dataPath = Path.Combine(FileSystem.AppDataDirectory, "makeready-data.json");
        builder.Services.AddSingleton(new JsonDataStore(dataPath));

        builder.Services.AddScoped<FirearmService>();
        builder.Services.AddScoped<HitFactorService>();
        builder.Services.AddScoped<MaintenanceService>();
        builder.Services.AddScoped<ExportService>();
        builder.Services.AddScoped<AmmoService>();
        builder.Services.AddSingleton<AlertService>();
        builder.Services.AddSingleton<NavDrawerState>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
