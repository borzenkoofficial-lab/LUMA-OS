using Microsoft.UI.Xaml;

namespace Luma.App;

public partial class App : Application
{
    private MainWindow? _mainWindow;
    private DockWindow? _dockWindow;
    private MenuBarWindow? _menuBarWindow;
    private LaunchpadWindow? _launchpadWindow;
    private ControlCenterWindow? _controlCenterWindow;

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _mainWindow = new MainWindow();

        // Keep the Windows desktop visible. LUMA's menu bar and Dock are
        // independent shell surfaces; the dashboard opens on explicit request.
        _menuBarWindow = new MenuBarWindow(
            OpenDashboard,
            OpenLaunchpad,
            OpenSpotlight,
            OpenControlCenter);
        _menuBarWindow.Activate();

        _dockWindow = new DockWindow(OpenDashboard, OpenLaunchpad);
        _dockWindow.Activate();
    }

    public void RefreshDock() => _dockWindow?.RefreshItems();

    private void OpenDashboard() => _mainWindow?.Activate();

    private void OpenSpotlight() => _mainWindow?.ShowSpotlight();

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

    private void OpenControlCenter()
    {
        if (_controlCenterWindow is not null)
        {
            _controlCenterWindow.Activate();
            return;
        }

        _controlCenterWindow = new ControlCenterWindow();
        _controlCenterWindow.Closed += (_, _) => _controlCenterWindow = null;
        _controlCenterWindow.Activate();
    }
}
