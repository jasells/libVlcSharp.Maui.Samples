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
}