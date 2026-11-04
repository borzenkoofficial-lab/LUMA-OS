using System.Collections.ObjectModel;
using System.Diagnostics;
using Luma.App.Services;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Graphics;

namespace Luma.App;

public sealed partial class DockWindow : Window
{
    private readonly Action _openDashboard;
    private readonly DockService _dockService = new();
    private readonly ObservableCollection<DockItem> _items = new();

    public DockWindow(Action openDashboard)
    {
        _openDashboard = openDashboard;
        InitializeComponent();
        DockItems.ItemsSource = _items;
        ConfigureWindow();
        RefreshItems();
    }

    public void RefreshItems()
    {
        _items.Clear();
        foreach (var app in _dockService.Load())
        {
            _items.Add(new DockItem(
                app.Name,
                app.ShortcutPath,
                string.IsNullOrWhiteSpace(app.Name) ? "•" : app.Name.Trim()[0].ToString().ToUpperInvariant()));
        }

        EmptyDockText.Visibility = _items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
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

        const int width = 570;
        const int height = 86;
        var workArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary).WorkArea;
        var x = workArea.X + Math.Max(0, (workArea.Width - width) / 2);
        var y = workArea.Y + Math.Max(0, workArea.Height - height - 12);
        appWindow.MoveAndResize(new RectInt32(x, y, width, height));
    }

    private void LumaButton_Click(object sender, RoutedEventArgs e) => _openDashboard();

    private void LaunchApp_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Microsoft.UI.Xaml.Controls.Button { Tag: string shortcutPath })
            return;

        if (!File.Exists(shortcutPath))
        {
            RefreshItems();
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo(shortcutPath) { UseShellExecute = true });
        }
        catch
        {
            // Keep the Dock alive if an application shortcut cannot be launched.
        }
    }

    private sealed record DockItem(string Name, string ShortcutPath, string Initial);
}
