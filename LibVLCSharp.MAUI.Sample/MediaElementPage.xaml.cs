namespace LibVLCSharp.MAUI.Sample;

public partial class MediaElementPage : ContentPage
{
	public MediaElementPage()
	{
		InitializeComponent();
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ((MainViewModel)BindingContext).OnVideoViewInitialized();
#if !WINDOWS
        ((MainViewModel)BindingContext).OnAppearing();
#endif
    }
}