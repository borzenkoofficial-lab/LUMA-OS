using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using Luma.App.Services;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Graphics;

namespace Luma.App;

public sealed partial class LaunchpadWindow : Window
{
    private readonly AppCatalogService _catalog = new();
    private readonly IReadOnlyList<AppShortcut> _allApps;
    private readonly ObservableCollection<LaunchpadItem> _visibleApps = new();

    public LaunchpadWindow()
    {
        InitializeComponent();
        _allApps = _catalog.Discover();
        AppsGrid.ItemsSource = _visibleApps;
        ConfigureWindow();
        ApplyFilter();
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

        const int width = 980;
        const int height = 720;
        var workArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary).WorkArea;
        var x = workArea.X + Math.Max(0, (workArea.Width - width) / 2);
        var y = workArea.Y + Math.Max(0, (workArea.Height - height) / 2);
        appWindow.MoveAndResize(new RectInt32(x, y, width, height));
    }

    private void ApplyFilter()
    {
        var query = SearchBox.Text?.Trim() ?? string.Empty;
        var matches = string.IsNullOrEmpty(query)
            ? _allApps
            : _allApps.Where(app =>
                app.Name.Contains(query, StringComparison.CurrentCultureIgnoreCase) ||
                app.Category.Contains(query, StringComparison.CurrentCultureIgnoreCase)).ToArray();

        _visibleApps.Clear();
        foreach (var app in matches)
        {
            var initial = string.IsNullOrWhiteSpace(app.Name)
                ? "•"
                : app.Name.Trim()[0].ToString().ToUpperInvariant();
            var item = new LaunchpadItem(app.Name, app.ShortcutPath, initial);
            _visibleApps.Add(item);
            _ = LoadIconAsync(item);
        }

        StatusText.Text = _allApps.Count == 0
            ? "No Start Menu shortcuts were found."
            : $"{_visibleApps.Count} applications";
    }

    private static async Task LoadIconAsync(LaunchpadItem item)
    {
        item.Icon = await ShellIconService.LoadShortcutIconAsync(item.ShortcutPath);
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

    private void AppsGrid_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not LaunchpadItem app)
            return;

        if (!File.Exists(app.ShortcutPath))
        {
            StatusText.Text = "This application shortcut is no longer available.";
            ApplyFilter();
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo(app.ShortcutPath) { UseShellExecute = true });
            StatusText.Text = $"Launching {app.Name}…";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Could not launch {app.Name}: {ex.Message}";
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();

    private sealed class LaunchpadItem : INotifyPropertyChanged
    {
        private ImageSource? _icon;

        public LaunchpadItem(string name, string shortcutPath, string initial)
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
