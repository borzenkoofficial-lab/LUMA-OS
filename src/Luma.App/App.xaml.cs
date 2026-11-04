using Microsoft.UI.Xaml;

namespace Luma.App;

public partial class App : Application
{
    private MainWindow? _mainWindow;
    private DockWindow? _dockWindow;
    private LaunchpadWindow? _launchpadWindow;

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _mainWindow = new MainWindow();
        _mainWindow.Activate();

        _dockWindow = new DockWindow(OpenDashboard, OpenLaunchpad);
        _dockWindow.Activate();
    }

    public void RefreshDock() => _dockWindow?.RefreshItems();

    private void OpenDashboard()
    {
        _mainWindow?.Activate();
    }

    private void OpenLaunchpad()
    {
        if (_launchpadWindow is not null)
        {
            _launchpadWindow.Activate();
            return;
        }

        _launchpadWindow = new LaunchpadWindow();
        _launchpadWindow.Closed += (_, _) => _launchpadWindow = null;
        _launchpadWindow.Activate();
    }
}
