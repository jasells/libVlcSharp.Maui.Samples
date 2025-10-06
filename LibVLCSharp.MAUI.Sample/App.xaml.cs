
using System.Diagnostics;

namespace LibVLCSharp.MAUI.Sample;

public partial class App : Application
{
    internal static StateService StateService { get; } = new StateService();

    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();
    }

    protected override Window CreateWindow(IActivationState activationState)
    {
        var win = base.CreateWindow(activationState);

        win.Activated += (s, e) =>
        {
            //((AppShell)win.Page).OnAppearing();
            Debug.WriteLine("Window Activated");
            StateService.State = AppState.Activated;

            StateService.OnStateChanged();
        };

        win.Backgrounding += (s, e) =>
        {
            //((AppShell)win.Page).OnDisappearing();
            Debug.WriteLine("Window Backgrounding");
        };

        win.Deactivated += (s, e) =>
        {
            Debug.WriteLine("Window Deactivated");
            StateService.State = AppState.Deactivated;

            StateService.OnStateChanged();

        };
        return win;
    }
}