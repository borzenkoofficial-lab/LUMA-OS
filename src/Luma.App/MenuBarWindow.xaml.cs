using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Graphics;

namespace Luma.App;

public sealed partial class MenuBarWindow : Window
{
    private readonly Action _openDashboard;
    private readonly Action _openLaunchpad;
    private readonly Action _openSpotlight;
    private readonly Action _openControlCenter;
    private readonly Action _openFileManager;
    private readonly DispatcherQueueTimer _clockTimer;

    public MenuBarWindow(
        Action openDashboard,
        Action openLaunchpad,
        Action openSpotlight,
        Action openControlCenter,
        Action openFileManager)
    {
        _openDashboard = openDashboard;
        _openLaunchpad = openLaunchpad;
        _openSpotlight = openSpotlight;
        _openControlCenter = openControlCenter;
        _openFileManager = openFileManager;

        InitializeComponent();
        ConfigureWindow();
        UpdateClock();

        _clockTimer = DispatcherQueue.GetForCurrentThread().CreateTimer();
        _clockTimer.Interval = TimeSpan.FromSeconds(15);
        _clockTimer.IsRepeating = true;
        _clockTimer.Tick += (_, _) => UpdateClock();
        _clockTimer.Start();
        Closed += (_, _) => _clockTimer.Stop();
    }

    private void ConfigureWindow()
    {
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);

        if (appWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.SetBorderAndTitleBar(false, false);
            presenter.IsResizable = false;
            presenter.IsMaximizable = false;
            presenter.IsMinimizable = false;
            presenter.IsAlwaysOnTop = true;
        }

        const int height = 36;
        var workArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary).WorkArea;
        appWindow.MoveAndResize(new RectInt32(workArea.X, workArea.Y, workArea.Width, height));
    }

    private void UpdateClock()
    {
        var now = DateTime.Now;
        DateText.Text = now.ToString("ddd, MMM d");
        TimeText.Text = now.ToString("h:mm tt");
    }

    private void Dashboard_Click(object sender, RoutedEventArgs e) => _openDashboard();
    private void Launchpad_Click(object sender, RoutedEventArgs e) => _openLaunchpad();
    private void Spotlight_Click(object sender, RoutedEventArgs e) => _openSpotlight();
    private void ControlCenter_Click(object sender, RoutedEventArgs e) => _openControlCenter();
    private void Files_Click(object sender, RoutedEventArgs e) => _openFileManager();
}
