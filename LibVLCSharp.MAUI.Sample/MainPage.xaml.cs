using LibVLCSharp.Shared;
using System.Diagnostics;

namespace LibVLCSharp.MAUI.Sample
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            // todo: need to POC a shell-nav scenario where we create a new instance of this 
            //page so we can see how the lifecycle events work. Probably need to move this to 
            // OnNavigatedTo override and remove handler in OnNavigatedFrom override...
            App.StateService.StateChanged += (s, e) =>
            {
                Debug.WriteLine($"==== App StateChanged: {App.StateService.State}");
                if (App.StateService.State == AppState.Activated)
                {
                   //Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), OnAppearing);
                   OnAppearing();
                }
                else if (App.StateService.State == AppState.Deactivated)
                {
                    OnDisappearing();
                }
            };
        }

        protected override void OnAppearing()
        {
            Debug.WriteLine("==== MainPage OnAppearing");
            base.OnAppearing();
#if !WINDOWS
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

        protected override void OnDisappearing()
        {
            Debug.WriteLine("==== MainPage OnDisappearing");
            base.OnDisappearing();
            ((MainViewModel)BindingContext).OnDisappearing();
            vid.MediaPlayerChanged -= VideoView_MediaPlayerChanged;
        }

        private void VideoView_MediaPlayerChanged(object sender, MediaPlayerChangedEventArgs e)
        {
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
    }
}