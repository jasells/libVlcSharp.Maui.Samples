using LibVLCSharp.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibVLCSharp.MAUI;

public class VideoView : ContentView
{
    /// <summary>
    /// Located when the parent is set, <c>null</c> until then.
    /// **todo/improvement: make this property raise PropertyChanging and PropertyChanged events
    /// </summary>
    public Page ParentPage { get; protected set; } = null;

    /// <summary>
    /// Raised when a new MediaPlayer is set and will be attached to the view
    /// </summary>
    public event EventHandler<MediaPlayerChangingEventArgs>? MediaPlayerChanging;

    /// <summary>
    /// Raised when a new MediaPlayer is set and attached to the view
    /// </summary>
    public event EventHandler<MediaPlayerChangedEventArgs>? MediaPlayerChanged;

    /// <summary>
    /// Xamarin.Forms MediaPlayer databinded property
    /// </summary>
    public static readonly BindableProperty MediaPlayerProperty = BindableProperty.Create(nameof(MediaPlayer),
            typeof(LibVLCSharp.Shared.MediaPlayer),
            typeof(VideoSurface),
            propertyChanging: OnMediaPlayerChanging,
            propertyChanged: OnMediaPlayerChanged);

    /// <summary>
    /// The MediaPlayer object attached to this view
    /// </summary>
    public LibVLCSharp.Shared.MediaPlayer? MediaPlayer
    {
        get { return GetValue(MediaPlayerProperty) as LibVLCSharp.Shared.MediaPlayer; }
        set { SetValue(MediaPlayerProperty, value); }
    }

    private static void OnMediaPlayerChanging(BindableObject bindable, object oldValue, object newValue)
    {
        var videoView = (VideoView)bindable;
        Debug.WriteLine("OnMediaPlayerChanging");
        videoView.MediaPlayerChanging?.Invoke(videoView, new MediaPlayerChangingEventArgs(oldValue as LibVLCSharp.Shared.MediaPlayer, newValue as LibVLCSharp.Shared.MediaPlayer));
    }

    private static void OnMediaPlayerChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var videoView = (VideoView)bindable;
        Debug.WriteLine("OnMediaPlayerChanged");

        if (newValue is LibVLCSharp.Shared.MediaPlayer newPlayer)
        {
            videoView._surface.MediaPlayer = newPlayer;

            videoView.MediaPlayerChanged?.Invoke(videoView, new MediaPlayerChangedEventArgs(oldValue as LibVLCSharp.Shared.MediaPlayer, newPlayer));
        }
    }

    public VideoView()
    {
        Content = _surface = new VideoSurface
        {
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
        };
        ((VideoSurface)Content).MediaPlayerChanging += (s, e) => MediaPlayerChanging?.Invoke(this, e);
        ((VideoSurface)Content).MediaPlayerChanged += (s, e) => MediaPlayerChanged?.Invoke(this, e);
    
        _appStateService = IPlatformApplication.Current.Services.GetService<IStateService>();

#if Windows
        // we need to init vlc when the VideoSurface is initialized
        _surface.HandlerChanged += VideoSurface_HandlerChanged;
#endif
    }

    protected override void OnParentSet()
    {
        base.OnParentSet();

        if (Parent is Page p)
        {
            SetupParentPage(p);
        }
        else
        {
            Debug.WriteLine($"======= in {nameof(OnParentSet)}: {nameof(VideoView)} ParentPage should be set before using.");

            // have to find the root parent page, but the view tree is built leaf-up, not root down...
            if (Parent.Parent == null)
                Parent.ParentChanged += ParentPageHandler;
            else
                ParentPageHandler(Parent.Parent, EventArgs.Empty);
        }
    }

    protected virtual void ParentPageHandler(object sender, EventArgs e)
    {
        var newParent = sender as Element;

        Debug.WriteLine($"======= {nameof(VideoView)}.Parent parent changed: {sender?.GetType().Name}");

        // remove this handler
        if (newParent != null)
        {
            newParent.ParentChanged -= ParentPageHandler;
        }

        if (newParent is Page p)
        {
            Debug.WriteLine($"======= {nameof(VideoView)}.ParentPage found: {p.GetType()}.");
            SetupParentPage(p);
        }
        else if (newParent?.Parent is Page p2)
        {
            Debug.WriteLine($"======= {nameof(VideoView)}.ParentPage found via .Parent: {p2.GetType()}.");
            SetupParentPage(p2);
        }
        else if (newParent.Parent == null)
        {
            Debug.WriteLine($"======= {nameof(VideoView)} ParentPage should be set before using, looking farther up UI tree.");
            newParent.ParentChanged += ParentPageHandler;
        }
        else
        {
            ParentPageHandler(newParent.Parent, EventArgs.Empty);
        }
    }

    protected virtual void SetupParentPage(Page parent)
    {
        if (ParentPage == null)
        {
            ParentPage = parent;
        }

        if (ParentPage == null) return;

        ParentPage.NavigatedTo += OnNavigatedTo;

        ParentPage.NavigatingFrom += OnNavigatedFrom;

        ParentPage.Appearing += (s, e) => OnAppearing();

        ParentPage.Disappearing += (s, e) => OnDisappearing();
    }

    private void OnNavigatedFrom(object sender, NavigatingFromEventArgs e)
    {
        // if the page is no longer on-screen, we should unsubscribe
        // only android?
        _appStateService.StateChanged -= AppStateChanged;
    }

    private void OnNavigatedTo(object sender, NavigatedToEventArgs e)
    {
        _appStateService.StateChanged += AppStateChanged;
    }

    private void AppStateChanged(object sender, AppState e)
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

    protected virtual void OnAppearing()
    {
        Debug.WriteLine("==== VideoView OnAppearing");
        //base.OnAppearing();
#if !WINDOWS //should be android?

        Content = _surface = new VideoSurface
        {
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
        };
#endif
#if !WINDOWS
        if (_vlcInitComplete == false)
            Initialize();
#endif
        _vlcInitComplete = true;
        Play();
        MediaPlayer.Position = lastPosition;

        _surface.MediaPlayerChanged += VideoView_MediaPlayerChanged;
        _surface.MediaPlayer = MediaPlayer;

#if !Windows // should be android?
        this.ForceLayout();
        this.InvalidateMeasure();
#endif
    }

    internal void Initialize(string[] swapchainOptions = null)
    {
        LibVLC = new LibVLC(enableDebugLogs: true, swapchainOptions);
        using var media = new Media(LibVLC, new Uri("http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4"));

        MediaPlayer = new Shared.MediaPlayer(LibVLC)
        {
            Media = media
        };
    }

    // this may need to be static?
    private LibVLC LibVLC { get; set; }


    protected virtual void OnDisappearing()
    {
        Debug.WriteLine("==== VideoView OnDisappearing");
        //base.OnDisappearing();
        //((MainViewModel)BindingContext).OnDisappearing();

        _surface.MediaPlayer.Pause();

        lastPosition = _surface.MediaPlayer.Position;

        _surface.MediaPlayer.Stop();

        _surface.MediaPlayerChanged -= VideoView_MediaPlayerChanged;
    }

    private void VideoView_MediaPlayerChanged(object sender, MediaPlayerChangedEventArgs e)
    {
        OnVideoViewInitialized();
    }

    public void OnVideoViewInitialized()
    {
        _isInitialized = true;
        Play();
    }

    private void Play()
    {
        if (IsLoaded && _isInitialized)
        {
            _surface.MediaPlayer.Play();
        }
    }

    //**this needs work!  needs to point at the _surface instance/type?
    private void VideoSurface_HandlerChanged(object sender, EventArgs e)
    {
#if WINDOWS
//this may not even be needed now that it is internal to libvlcsharp.maui ?
            var windowsView = ((LibVLCSharp.Platforms.Windows.VideoView)VideoSurface.Handler.PlatformView);

            windowsView.Initialized += (s, e) =>
            {
                Initialize(e.SwapChainOptions);
                OnAppearing();
            };
#endif
    }

    private VideoSurface _surface;
    private readonly IStateService _appStateService;
    private float lastPosition = 0;
    private bool _isInitialized = false;
    private bool _vlcInitComplete = false;
}
