using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LibVLCSharp.MAUI.Sample
{
    public enum AppState
    {
        Uninitialized,
        Activated,
        Deactivated,
    }

    public static class AppStateExtensions
    {
        public static Window SetupVlc(this Microsoft.Maui.Controls.Window rootMauiWindow,
                                   IStateService stateService)
        {
            rootMauiWindow.Activated += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine("Window Activated");
                stateService.State = AppState.Activated;
            };

            rootMauiWindow.Deactivated += (s, e) =>
            {
                System.Diagnostics.Debug.WriteLine("Window Deactivated");
                stateService.State = AppState.Deactivated;
            };

            return rootMauiWindow;
        }
    }

    internal class StateService : IStateService
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<AppState> StateChanged;

        public AppState State
        {
            get => _state;
            set
            {
                if (Set(ref _state, value))
                {
                    OnStateChanged();
                }
            }
        }
        private AppState _state = AppState.Uninitialized;

        private bool Set<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            // call now, no performance hit if no event subscribers.
            OnPropertyChanged(propertyName);

            return true;
        }

        /// <summary>
        /// Call this from a property setter after setting the backing field,
        /// calling <see cref="SetProperty{T}(ref T, T, string)"/>,
        /// or calling <see cref="OnPropertyChanged(string)"/>
        /// </summary>
        /// <param name="propertyName"></param>
        virtual protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }


        public void OnStateChanged() => StateChanged?.Invoke(this, State);
    }
}
