using System.ComponentModel;

namespace LibVLCSharp.MAUI.Sample;

public interface IAppStateManager : INotifyPropertyChanged
{
    AppState State { get; set; }

    event EventHandler<AppState> StateChanged;

    void OnStateChanged();
}