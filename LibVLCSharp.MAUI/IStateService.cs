using System.ComponentModel;

namespace LibVLCSharp.MAUI;

public interface IStateService : INotifyPropertyChanged
{
    AppState State { get; set; }

    event EventHandler<AppState> StateChanged;

    void OnStateChanged();
}