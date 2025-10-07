
using System.Diagnostics;

namespace LibVLCSharp.MAUI.Sample;

public partial class App : Application
{
    //internal static StateService StateService { get; } = new StateService();

    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();
    }

    protected override Window CreateWindow(IActivationState activationState)
    {
        return base.CreateWindow(activationState)
                   .SetupVlc(IPlatformApplication.Current.Services.GetRequiredService<IAppStateManager>());
    }
}