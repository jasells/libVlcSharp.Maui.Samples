using Microsoft.Extensions.Logging;

namespace LibVLCSharp.MAUI.Sample;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>((_) => new App().SetupVlcAppManager())
            .UseLibVLCSharp()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
            //register app-state-service for state management
            .Services.AddSingleton<IAppStateManager, AppStateManager>();

#if DEBUG
		    builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}