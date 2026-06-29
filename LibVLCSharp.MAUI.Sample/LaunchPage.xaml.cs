namespace LibVLCSharp.MAUI.Sample;

public partial class LaunchPage : ContentPage
{
	public LaunchPage()
	{
		InitializeComponent();
	}

	private async void OnGoToMainPageClicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync(nameof(MainPage));
	}

	private async void OnGoToMediaElementPageClicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync(nameof(MediaElementPage));
	}

	// Stress harness for VideoLAN issue #659 (managed ObjectDisposedException in VideoView.Detach).
	// Repeatedly navigates into the player page (starts playback) and back out, then forces both the
	// .NET and Java GCs so VideoView's finalizer runs Detach() against already-collected Java peers.
	// On the unpatched build this eventually crashes the process with an unhandled
	// ObjectDisposedException on the finalizer thread; on the patched (XDev fork) build the fix
	// swallows it and raises LibVLCSharpDiagnostics.Warning instead.
	private async void OnRunStressClicked(object sender, EventArgs e)
	{
		const int iterations = 100;
		StressBtn.IsEnabled = false;
		var rng = new Random();
		try
		{
			for (int i = 0; i < iterations; i++)
			{
				await Shell.Current.GoToAsync(nameof(MainPage));   // VideoView page (targets VideoView.Detach, the #659 fix path)
				await Task.Delay(rng.Next(1000, 5000));   // playback running; vary the race window
				await Shell.Current.GoToAsync("..");     // pop -> OnDisappearing -> Stop()+Dispose();
														 // VideoView itself is left to the GC

				//GC.Collect();
				//GC.WaitForPendingFinalizers();
#if ANDROID
				Java.Lang.JavaSystem.Gc();               // ask the ART GC to collect Java peers
#endif
				GC.Collect();
				GC.WaitForPendingFinalizers();           // run VideoView finalizer -> Detach()

				await Task.Delay(rng.Next(50, 200));

				var msg = $"[#659 stress] iteration {i + 1}/{iterations}";
				System.Diagnostics.Debug.WriteLine(msg);
				StressStatus.Text = msg;
			}
			StressStatus.Text = $"[#659 stress] completed {iterations} iterations without a crash";
		}
		catch (Exception ex)
		{
			// Navigation-level exceptions only. The target crash is an unhandled exception on the
			// finalizer thread, which kills the process and will NOT be caught here.
			System.Diagnostics.Debug.WriteLine($"[#659 stress] navigation exception: {ex}");
			StressStatus.Text = $"[#659 stress] nav exception: {ex.GetType().Name}";
		}
		finally
		{
			StressBtn.IsEnabled = true;
		}
	}
}