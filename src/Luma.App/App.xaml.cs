using Microsoft.UI.Xaml;

namespace Luma.App;

public partial class App : Application
{
    private MainWindow? _mainWindow;
    private DockWindow? _dockWindow;

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _mainWindow = new MainWindow();
        _mainWindow.Activate();

        _dockWindow = new DockWindow(OpenDashboard);
        _dockWindow.Activate();
    }

    public void RefreshDock() => _dockWindow?.RefreshItems();

    private void OpenDashboard()
    {
        _mainWindow?.Activate();
    }
}
