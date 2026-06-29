using Microsoft.Extensions.Logging;

namespace LibVLCSharp.MAUI.Sample
{
	public static class MauiProgram
	{
		public static MauiApp CreateMauiApp()
		{
			var builder = MauiApp.CreateBuilder();
			builder
				.UseMauiApp<App>()
				.UseLibVLCSharp()
				.ConfigureFonts(fonts =>
				{
					fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
					fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				});

#if DEBUG
			builder.Logging.AddDebug();
#endif

#if ANDROID_REPRO
			// Issue #659: the XDev fork's VideoView.Detach() swallows the managed GC-race
			// ObjectDisposedException and raises this warning instead of crashing. Each line logged
			// here is one race that was hit and survived. (This type only exists in the XDev fork.)
			LibVLCSharp.Shared.LibVLCSharpDiagnostics.Warning += (s, e) =>
				System.Diagnostics.Debug.WriteLine($"[#659 caught] {e.Source}: {e.Message}");
#endif

			return builder.Build();
		}
	}
}