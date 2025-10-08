using LibVLCSharp.Shared;
using System.Diagnostics;

namespace LibVLCSharp.MAUI.Sample;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        _appManager = IPlatformApplication.Current
                                          .Services
                                          .GetRequiredService<IAppStateManager>();
    }

    private void AppStateChanged(object s, AppState e)
    {
        Debug.WriteLine($"==== App StateChanged: {e}");
        if (e == AppState.Activated)
        {
            //Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), OnAppearing);
            OnAppearing();
        }
        else if (e == AppState.Deactivated)
        {
            OnDisappearing();
        }
    }

    protected override void OnAppearing()
    {
        Debug.WriteLine("==== MainPage OnAppearing");
        base.OnAppearing();

#if !WINDOWS
        // Android needs a new view created _every_ time the window re-appears to restart
        // rendering, so this code is primarily for Android. iOS seems to be able to re-use
        // the same view instance, but doing it this way works for both platforms,
        // without more complicated checks.
        Content = vid = new VideoView
        {
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
        };

        ((MainViewModel)BindingContext).OnAppearing();

        vid.MediaPlayerChanged += VideoView_MediaPlayerChanged;
        vid.MediaPlayer = ((MainViewModel)BindingContext).MediaPlayer;
#endif


        this.ForceLayout();
        this.InvalidateMeasure();
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        // if we navigate away from this page, we should unsubscribe
        _appManager.StateChanged -= AppStateChanged;
     
        base.OnNavigatedFrom(args);
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        // todo: need to POC a shell-nav scenario where we create a new instance of this 
        //page so we can see how the lifecycle events work. Probably need to move this to 
        // OnNavigatedTo override and remove handler in OnNavigatedFrom override...
        _appManager.StateChanged += AppStateChanged;

        base.OnNavigatedTo(args);
    }

    protected override void OnDisappearing()
    {
        Debug.WriteLine("==== MainPage OnDisappearing");
        base.OnDisappearing();
        ((MainViewModel)BindingContext).OnDisappearing();
#if ANDROID
        vid.MediaPlayerChanged -= VideoView_MediaPlayerChanged;
#endif
    }


    private void VideoView_MediaPlayerChanged(object sender, MediaPlayerChangedEventArgs e)
    {
        Debug.WriteLine("==== VideoView_MediaPlayerChanged");
        ((MainViewModel)BindingContext).OnVideoViewInitialized();
    }

    private void VideoView_HandlerChanged(object sender, EventArgs e)
    {
#if WINDOWS
        var windowsView = ((LibVLCSharp.Platforms.Windows.VideoView)VideoView.Handler.PlatformView);

        windowsView.Initialized += (s, e) =>
        {
            ((MainViewModel)BindingContext).Initialize(e.SwapChainOptions);
            ((MainViewModel)BindingContext).OnAppearing();
        };
#endif
    }

    private VideoView vid;
    private readonly IAppStateManager _appManager;
}