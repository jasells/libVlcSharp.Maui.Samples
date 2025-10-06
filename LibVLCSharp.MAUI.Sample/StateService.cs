using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibVLCSharp.MAUI.Sample
{
    public enum AppState
    {
        Uninitialized,
        Activated,
        Deactivated,
    }

    internal class StateService
    {
        public AppState State { get; set; } = AppState.Uninitialized;

        public event EventHandler StateChanged;

        public void OnStateChanged() => StateChanged?.Invoke(this, EventArgs.Empty);
    }
}
