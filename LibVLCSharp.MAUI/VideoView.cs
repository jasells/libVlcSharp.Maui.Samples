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
            videoView.surface.MediaPlayer = newPlayer;

            videoView.MediaPlayerChanged?.Invoke(videoView, new MediaPlayerChangedEventArgs(oldValue as LibVLCSharp.Shared.MediaPlayer, newPlayer));
        }
    }

    public VideoView()
    {
        Content = surface = new VideoSurface
        {
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill,
        };
        ((VideoSurface)Content).MediaPlayerChanging += (s, e) => MediaPlayerChanging?.Invoke(this, e);
        ((VideoSurface)Content).MediaPlayerChanged += (s, e) => MediaPlayerChanged?.Invoke(this, e);
    }

    private VideoSurface surface;

}
