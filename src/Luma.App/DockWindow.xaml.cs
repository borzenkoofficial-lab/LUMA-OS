using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using Luma.App.Services;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Windows.Graphics;

namespace Luma.App;

public sealed partial class DockWindow : Window
{
    private readonly Action _openDashboard;
    private readonly Action _openLaunchpad;
    private readonly DockService _dockService = new();
    private readonly ObservableCollection<DockItem> _items = new();

    public DockWindow(Action openDashboard, Action openLaunchpad)
    {
        _openDashboard = openDashboard;
        _openLaunchpad = openLaunchpad;
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
            var item = new DockItem(
                app.Name,
                app.ShortcutPath,
                string.IsNullOrWhiteSpace(app.Name) ? "•" : app.Name.Trim()[0].ToString().ToUpperInvariant());
            _items.Add(item);
            _ = LoadIconAsync(item);
        }

        EmptyDockText.Visibility = _items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private static async Task LoadIconAsync(DockItem item)
    {
        item.Icon = await ShellIconService.LoadShortcutIconAsync(item.ShortcutPath);
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

    private void LaunchpadButton_Click(object sender, RoutedEventArgs e) => _openLaunchpad();

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

    private sealed class DockItem : INotifyPropertyChanged
    {
        private ImageSource? _icon;

        public DockItem(string name, string shortcutPath, string initial)
        {
            Name = name;
            ShortcutPath = shortcutPath;
            Initial = initial;
        }

        public string Name { get; }
        public string ShortcutPath { get; }
        public string Initial { get; }

        public ImageSource? Icon
        {
            get => _icon;
            set
            {
                if (ReferenceEquals(_icon, value))
                    return;
                _icon = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Icon)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
