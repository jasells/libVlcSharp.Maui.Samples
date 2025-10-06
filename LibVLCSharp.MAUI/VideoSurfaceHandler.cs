using Microsoft.Maui.Handlers;

namespace LibVLCSharp.MAUI
{
    /// <summary>
    /// 
    /// </summary>
    public partial class VideoSurfaceHandler
    {
        /// <summary>
        /// 
        /// </summary>
        public static IPropertyMapper<VideoSurface, VideoSurfaceHandler> PropertyMapper = new PropertyMapper<VideoSurface, VideoSurfaceHandler>(ViewMapper)
        {
            [nameof(VideoSurface.MediaPlayer)] = MapMediaPlayer
        };

        /// <summary>
        /// 
        /// </summary>
        public VideoSurfaceHandler() : base(PropertyMapper)
        {
        }

        /// <summary>
        /// Attach mediaplayer to the native view
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="view"></param>
        public static void MapMediaPlayer(VideoSurfaceHandler handler, VideoSurface view)
        {
            handler.PlatformView.MediaPlayer = view.MediaPlayer;
        }
    }
}
