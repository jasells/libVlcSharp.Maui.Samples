using LibVLCSharp.Shared;
using System.ComponentModel;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Threading;

namespace LibVLCSharp.MAUI.Sample
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public MainViewModel()
        {
#if !WINDOWS
            Initialize();
#endif
        }

        private LibVLC LibVLC { get; set; }

        private Shared.MediaPlayer _mediaPlayer;
        public Shared.MediaPlayer MediaPlayer
        {
            get => _mediaPlayer;
            private set => Set(nameof(MediaPlayer), ref _mediaPlayer, value);
        }

        private bool IsLoaded { get; set; }
        private bool IsVideoViewInitialized { get; set; }

        private void Set<T>(string propertyName, ref T field, T value)
        {
            if (field == null && value != null || field != null && !field.Equals(value))
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        internal void Initialize(string[] swapchainOptions = null)
        {
            LibVLC = new LibVLC(enableDebugLogs: true, swapchainOptions);
            using var media = new Media(LibVLC, new Uri("https://download.blender.org/peach/bigbuckbunny_movies/BigBuckBunny_320x180.mp4"));

            MediaPlayer = new Shared.MediaPlayer(LibVLC)
            {
                Media = media
            };
        }

        public void OnAppearing()
        {
            IsLoaded = true;
            Play();
        }

        internal async void OnDisappearing()
        {
            MediaPlayer.Stop();
            // disposing the player here crashes on Android, when using MediaElement, but not VideoView (in this sample stack).
            // I originally observed the problem in a more complex app with VideoView, so this seems related to how quickly the UI 
            // is torn down after/before stopping playback, i.e. a race condition?  Not sure, other insight needed.
            //
            // with Android 15 / Gal-S24 +  Android 13 / Pixel-4, the error observed is:
            // [libc] ../../lib/media_player.c:197: input_thread_t *libvlc_get_input_thread(libvlc_media_player_t *): assertion "p_mi" failed
            // [libc] Fatal signal 6(SIGABRT), code - 1(SI_QUEUE) in tid 5532(rp.maui.samplex), pid 5532(rp.maui.samplex)
            //
            // In a similar scenario that was the driver for this repro, 
            // I see repeated:
            //[Adreno] DequeueBuffer: dequeueBuffer failed
            //[BufferQueueProducer][SurfaceView[xxxx/ crc64a16e7a643bcdc9a6.MainActivity]#1(BLAST Consumer)1](id:c4700000004,api:0,p:-1,c:3143) query: BufferQueue has been abandoned
            //[BufferQueueProducer][SurfaceView[xxxx/ crc64a16e7a643bcdc9a6.MainActivity]#1(BLAST Consumer)1](id:c4700000004,api:0,p:-1,c:3143) dequeueBuffer: BufferQueue has been abandoned
            //[Adreno] DequeueBuffer: dequeueBuffer failed
            // after MediaPlayer.Stop();
            //
            // and then this crash on MediaPlayer.Dispose():
            // 
            //java_vm_ext.cc:616] JNI DETECTED ERROR IN APPLICATION: a thread (tid 3570 is making JNI calls without being attached
            //[xxxx] java_vm_ext.cc:616]     in call to GetJavaVM
            //The thread 23 has exited with code 0(0x0).
            //[xxxx] runtime.cc:709] Runtime aborting...
            //[xxxx] runtime.cc:709] Dumping all threads without mutator lock held
            //[xxxx] runtime.cc:709] All threads:
            //[xxxx] runtime.cc:709] DALVIK THREADS(56):
            //[xxxx] runtime.cc:709] "main" prio = 10 tid = 1 Native
            //[xxxx] runtime.cc:709]   | group = "" sCount = 1 ucsCount = 0 flags = 1 obj = 0x72ec0d58 self = 0xb400006fcb43d380
            //[xxxx] runtime.cc:709]   | sysTid = 3143 nice = -10 cgrp = system sched = 0 / 0 handle = 0x7109f204f8
            //[xxxx] runtime.cc:709]   | state = S schedstat = (6614777401 492116611 17210 ) utm = 627 stm = 33 core = 7 HZ = 100
            //[xxxx] runtime.cc:709]   | stack = 0x7fedb9e000 - 0x7fedba0000 stackSize = 8188KB
            //[xxxx] runtime.cc:709]   | held mutexes =
            //[xxxx] runtime.cc:709]   native: #00 pc 0004de5c  /apex/com.android.runtime/lib64/bionic/libc.so (syscall+28) (BuildId: 058e3ec96fa600fb840a6a6956c6b64e)
            //[xxxx] runtime.cc:709]   native: #01 pc 004857c4  /data/app/~~rH3ryEDRzUM4NYA-o8B2qA==/com.bxxxx-9NdSaBIHsg1Ibx0R2XK3bg==/lib/arm64/libvlc.so (vlc_cond_wait+168) (BuildId: 260711dbe9d297b2b4332a10a46ba70771b1f5a7)
            //[xxxx] runtime.cc:709]   native: #02 pc 00485d50  /data/app/~~rH3ryEDRzUM4NYA-o8B2qA==/com.bxxxx-9NdSaBIHsg1Ibx0R2XK3bg==/lib/arm64/libvlc.so (vlc_sem_wait+80) (BuildId: 260711dbe9d297b2b4332a10a46ba70771b1f5a7)
            //[xxxx] runtime.cc:709]   native: #03 pc 00491124  /data/app/~~rH3ryEDRzUM4NYA-o8B2qA==/com.bxxxx-9NdSaBIHsg1Ibx0R2XK3bg==/lib/arm64/libvlc.so (vlc_join+32) (BuildId: 260711dbe9d297b2b4332a10a46ba70771b1f5a7)
            //[xxxx] runtime.cc:709]   native: #04 pc 004653d0  /data/app/~~rH3ryEDRzUM4NYA-o8B2qA==/com.bxxxx-9NdSaBIHsg1Ibx0R2XK3bg==/lib/arm64/libvlc.so (input_Close+32) (BuildId: 260711dbe9d297b2b4332a10a46ba70771b1f5a7)
            //[xxxx] runtime.cc:709]   native: #05 pc 00448800  /data/app/~~rH3ryEDRzUM4NYA-o8B2qA==/com.bxxxx-9NdSaBIHsg1Ibx0R2XK3bg==/lib/arm64/libvlc.so (libvlc_media_player_release+228) (BuildId: 260711dbe9d297b2b4332a10a46ba70771b1f5a7)
            //[xxxx] runtime.cc:709]   native: #06 pc 001a93f0  /data/app/~~rH3ryEDRzUM4NYA-o8B2qA==/com.bxxxx-9NdSaBIHsg1Ibx0R2XK3bg==/lib/arm64/libmonosgen-2.0.so (???) (BuildId: 98be37ca76fe1d5ef9cdea8688fefa2e0bcd2801)
            //[xxxx] runtime.cc:709]   native: #07 pc 001a7e70  /data/app/~~rH3ryEDRzUM4NYA-o8B2qA==/com.bxxxx-9NdSaBIHsg1Ibx0R2XK3bg==/lib/arm64/libmonosgen-2.0.so (???) (BuildId: 98be37ca76fe1d5ef9cdea8688fefa2e0bcd2801)
            //[xxxx] runtime.cc:709]   native: #08 pc 0019d3e8  /data/app/~~rH3ryEDRzUM4NYA-o8B2qA==/com.bxxxx-9NdSaBIHsg1Ibx0R2XK3bg==/lib/arm64/libmonosgen-2.0.so (???) (BuildId: 98be37ca76fe1d5ef9cdea8688fefa2e0bcd2801)
            //[xxxx] runtime.cc:709]   native: #09 pc 001a97d4  /data/app/~~rH3ryEDRzUM4NYA-o8B2qA==/com.bxxxx-9NdSaBIHsg1Ibx0R2XK3bg==/lib/arm64/libmonosgen-2.0.so (???) (BuildId: 98be37ca76fe1d5ef9cdea8688fefa2e0bcd2801)
            //[xxxx] runtime.cc:709]   native: #10 pc 001a9ee8  /data/app/~~rH3ryEDRzUM4NYA-o8B2qA==/com.bxxxx-9NdSaBIHsg1Ibx0R2XK3bg==/lib/arm64/libmonosgen-2.0.so (???) (BuildId: 98be37ca76fe1d5ef9cdea8688fefa2e0bcd2801)
            //[xxxx] runtime.cc:709]   native: #11 pc 0000f8e0  <anonymous:70f9331000> (???)
            MediaPlayer.Dispose();
            LibVLC.Dispose();
        }

        public void OnVideoViewInitialized()
        {
            IsVideoViewInitialized = true;
            Play();
        }

        private void Play()
        {
            if (IsLoaded && IsVideoViewInitialized)
            {
                MediaPlayer.Play();
            }
        }
    }
}
