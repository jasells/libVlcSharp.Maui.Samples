
using System.Diagnostics;

namespace LibVLCSharp.MAUI.Sample;

public partial class App : Application
{
    //internal static StateService StateService { get; } = new StateService();

    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();

        ChildAdded += (s, e) => Debug.WriteLine($"==== App ChildAdded: {e.GetType()}");
    }

    protected override Window CreateWindow(IActivationState activationState)
    {
        Debug.WriteLine("==== App CreateWindow");
        return base.CreateWindow(activationState);
                   //.SetupVlc(IPlatformApplication.Current.Services.GetRequiredService<IAppStateManager>());
    }
}