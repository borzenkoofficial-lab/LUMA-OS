using System.Collections.ObjectModel;
using Luma.App.Services;
using Luma.Core.Desktop;
using Luma.Core.Navigation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Pickers;

namespace Luma.App;

public sealed partial class MainWindow : Window
{
    private readonly ShellNavigationState _navigation = new();
    private readonly AppCatalogService _appCatalog = new();
    private readonly WindowManagerService _windowManager = new();
    private readonly DockService _dockService = new();
    private readonly DesktopPreferencesService _desktopPreferences = new();
    private readonly DesktopLayoutService _desktopLayoutService = new();
    private readonly DesktopIconLayoutState _desktopLayout = new();
    private readonly ObservableCollection<AppShortcut> _desktopApps = new();
    private readonly ObservableCollection<AppShortcut> _visibleApps = new();
    private readonly ObservableCollection<AppShortcut> _pinnedApps = new();
    private readonly ObservableCollection<AppShortcut> _spotlightApps = new();
    private readonly ObservableCollection<ManagedWindow> _managedWindows = new();
    private IReadOnlyList<AppShortcut> _allApps = Array.Empty<AppShortcut>();
    private bool _updatingCategories;

    public MainWindow()
    {
        InitializeComponent();
        AppsList.ItemsSource = _visibleApps;
        DesktopItemsList.ItemsSource = _desktopApps;
        DockItemsControl.ItemsSource = _pinnedApps;
        SpotlightResults.ItemsSource = _spotlightApps;
        WindowItemsList.ItemsSource = _managedWindows;

        foreach (var app in _dockService.Load())
            _pinnedApps.Add(app);

        UpdateDockEmptyState();
        RestoreWallpaper();
        _desktopLayout.Restore(_desktopLayoutService.Load());
    }

    private void Navigate(ShellSection section, string eyebrow, string heading, string description)
    {
        _navigation.NavigateTo(section);
        EyebrowText.Text = eyebrow;
        HeadingText.Text = heading;
        DescriptionText.Text = description;

        var showApplications = section == ShellSection.Applications;
        var showDesktop = section == ShellSection.Desktop;
        var showWindows = section == ShellSection.Windows;
        OverviewPanel.Visibility = showApplications || showDesktop || showWindows ? Visibility.Collapsed : Visibility.Visible;
        ApplicationsPanel.Visibility = showApplications ? Visibility.Visible : Visibility.Collapsed;
        DesktopPanel.Visibility = showDesktop ? Visibility.Visible : Visibility.Collapsed;
        WindowsPanel.Visibility = showWindows ? Visibility.Visible : Visibility.Collapsed;

        if (showApplications && _allApps.Count == 0)
            RefreshApplications();
        if (showDesktop)
            RefreshDesktopShortcuts();
        if (showWindows)
            RefreshWindows();
    }

    private void RefreshDesktopShortcuts()
    {
        try
        {
            var shortcuts = _appCatalog.DiscoverDesktopShortcuts();
            _desktopLayout.Synchronize(shortcuts.Select(item => item.ShortcutPath));
            ApplyDesktopLayout(shortcuts);
            _desktopLayoutService.Save(_desktopLayout.OrderedPaths);
            DesktopShortcutsStatus.Text = shortcuts.Count == 0
                ? "No desktop shortcuts found."
                : $"{shortcuts.Count} desktop shortcuts • order saved locally.";
        }
        catch (Exception ex)
        {
            DesktopShortcutsStatus.Text = $"Could not read desktop shortcuts: {ex.Message}";
        }
    }

    private void RefreshApplications()
    {
        try
        {
            _allApps = _appCatalog.Discover();
            UpdateCategoryFilter();
            ApplyAppFilter();
            AppsStatusText.Text = _allApps.Count == 0
                ? "No Start Menu shortcuts found. Packaged apps without shortcuts are not listed yet."
                : $"Found {_allApps.Count} Start Menu shortcuts.";
            RefreshSpotlightResults();
        }
        catch (Exception ex)
        {
            AppsStatusText.Text = $"Could not read the Start Menu: {ex.Message}";
        }
    }

    private void ApplyAppFilter()
    {
        var query = AppSearchBox.Text?.Trim() ?? string.Empty;
        var selectedCategory = AppCategoryFilter.SelectedItem as string;
        var matches = _allApps.Where(app =>
                (string.IsNullOrEmpty(query) || app.Name.Contains(query, StringComparison.CurrentCultureIgnoreCase)) &&
                (string.IsNullOrEmpty(selectedCategory) || selectedCategory == "All categories" || app.Category == selectedCategory))
            .ToArray();

        _visibleApps.Clear();
        foreach (var app in matches)
            _visibleApps.Add(app);

        if (_allApps.Count > 0)
            AppsStatusText.Text = $"{_visibleApps.Count} of {_allApps.Count} applications";
    }

    private void UpdateCategoryFilter()
    {
        _updatingCategories = true;
        var selected = AppCategoryFilter.SelectedItem as string;
        AppCategoryFilter.Items.Clear();
        AppCategoryFilter.Items.Add("All categories");
        foreach (var category in _allApps.Select(app => app.Category).Distinct(StringComparer.CurrentCultureIgnoreCase).OrderBy(value => value))
            AppCategoryFilter.Items.Add(category);
        AppCategoryFilter.SelectedItem = selected is not null && AppCategoryFilter.Items.Contains(selected) ? selected : "All categories";
        _updatingCategories = false;
    }

    private void AppCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_updatingCategories) ApplyAppFilter();
    }

    private void UnpinDockApp_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string path }) return;
        var pinned = _pinnedApps.FirstOrDefault(app => string.Equals(app.ShortcutPath, path, StringComparison.OrdinalIgnoreCase));
        if (pinned is null) return;
        _pinnedApps.Remove(pinned);
        PersistDock();
    }

    private void RefreshSpotlightResults()
    {
        var query = SpotlightSearchBox.Text?.Trim() ?? string.Empty;
        var matches = string.IsNullOrWhiteSpace(query)
            ? Array.Empty<AppShortcut>()
            : _allApps.Where(app => app.Name.Contains(query, StringComparison.CurrentCultureIgnoreCase)).Take(12).ToArray();

        _spotlightApps.Clear();
        foreach (var app in matches)
            _spotlightApps.Add(app);

        SpotlightStatusText.Text = string.IsNullOrWhiteSpace(query)
            ? "Search your Start Menu applications"
            : $"{_spotlightApps.Count} matching applications";
    }

    private void PinApplication(string shortcutPath)
    {
        var app = _allApps.FirstOrDefault(item => string.Equals(item.ShortcutPath, shortcutPath, StringComparison.OrdinalIgnoreCase));
        if (app is null)
        {
            AppsStatusText.Text = "Application not found. Refresh the catalog and try again.";
            return;
        }

        if (_pinnedApps.Any(item => string.Equals(item.ShortcutPath, app.ShortcutPath, StringComparison.OrdinalIgnoreCase)))
        {
            AppsStatusText.Text = $"{app.Name} is already in the Dock.";
            return;
        }

        _pinnedApps.Add(app);
        PersistDock();
        AppsStatusText.Text = $"{app.Name} pinned to Dock.";
    }

    private void PersistDock()
    {
        try
        {
            _dockService.Save(_pinnedApps);
            UpdateDockEmptyState();
        }
        catch (Exception ex)
        {
            AppsStatusText.Text = $"Could not save Dock: {ex.Message}";
        }
    }

    private void UpdateDockEmptyState() =>
        DockEmptyText.Visibility = _pinnedApps.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

    private void LaunchShortcut(string shortcutPath, Action<string> report)
    {
        var app = _allApps.Concat(_pinnedApps).Concat(_desktopApps)
            .FirstOrDefault(item => string.Equals(item.ShortcutPath, shortcutPath, StringComparison.OrdinalIgnoreCase));

        if (app is null)
        {
            report("Application shortcut not found. Refresh the catalog and try again.");
            return;
        }

        try
        {
            _appCatalog.Launch(app);
            report($"Launched {app.Name}.");
        }
        catch (Exception ex)
        {
            report($"Could not launch {app.Name}: {ex.Message}");
        }
    }

    private void RestoreWallpaper()
    {
        var path = _desktopPreferences.LoadWallpaperPath();
        if (path is null)
        {
            WallpaperStatusText.Text = "No custom wallpaper selected.";
            return;
        }

        try
        {
            WorkspaceWallpaper.Source = new BitmapImage(new Uri(path));
            WallpaperStatusText.Text = $"Current wallpaper: {Path.GetFileName(path)}";
        }
        catch (Exception ex)
        {
            WallpaperStatusText.Text = $"Saved wallpaper could not be displayed: {ex.Message}";
        }
    }

    private async void ChooseWallpaper_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".bmp");

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
            var file = await picker.PickSingleFileAsync();
            if (file is null)
                return;

            _desktopPreferences.SaveWallpaperPath(file.Path);
            WorkspaceWallpaper.Source = new BitmapImage(new Uri(file.Path));
            WallpaperStatusText.Text = $"Current wallpaper: {file.Name}";
        }
        catch (Exception ex)
        {
            WallpaperStatusText.Text = $"Could not set wallpaper: {ex.Message}";
        }
    }

    private void RemoveWallpaper_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _desktopPreferences.ClearWallpaperPath();
            WorkspaceWallpaper.Source = null;
            WallpaperStatusText.Text = "Custom wallpaper removed.";
        }
        catch (Exception ex)
        {
            WallpaperStatusText.Text = $"Could not remove wallpaper preference: {ex.Message}";
        }
    }

    private void ApplyDesktopLayout(IReadOnlyList<AppShortcut> discovered)
    {
        var byPath = discovered.ToDictionary(item => item.ShortcutPath, StringComparer.OrdinalIgnoreCase);
        _desktopApps.Clear();
        foreach (var path in _desktopLayout.OrderedPaths)
        {
            if (byPath.TryGetValue(path, out var shortcut))
                _desktopApps.Add(shortcut);
        }
    }

    private void MoveDesktopShortcut(object sender, int offset)
    {
        var path = sender switch
        {
            Button { Tag: string buttonPath } => buttonPath,
            MenuFlyoutItem { Tag: string menuPath } => menuPath,
            _ => null
        };

        if (path is null || !_desktopLayout.Move(path, offset))
        {
            DesktopShortcutsStatus.Text = "Shortcut is already at that edge or no longer exists.";
            return;
        }

        try
        {
            _desktopLayoutService.Save(_desktopLayout.OrderedPaths);
            var current = _desktopApps.ToArray();
            ApplyDesktopLayout(current);
            DesktopShortcutsStatus.Text = "Desktop icon order saved.";
        }
        catch (Exception ex)
        {
            DesktopShortcutsStatus.Text = $"Could not save desktop layout: {ex.Message}";
        }
    }

    private void DesktopShortcutMoveUp_Click(object sender, RoutedEventArgs e) => MoveDesktopShortcut(sender, -1);
    private void DesktopShortcutMoveDown_Click(object sender, RoutedEventArgs e) => MoveDesktopShortcut(sender, 1);

    private void DesktopItems_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs e)
    {
        _desktopLayout.Restore(_desktopApps.Select(item => item.ShortcutPath));

        try
        {
            _desktopLayoutService.Save(_desktopLayout.OrderedPaths);
            DesktopShortcutsStatus.Text = "Desktop order updated and saved.";
        }
        catch (Exception ex)
        {
            DesktopShortcutsStatus.Text = $"The new order is active, but could not be saved: {ex.Message}";
        }
    }

    private void RefreshDesktop_Click(object sender, RoutedEventArgs e) => RefreshDesktopShortcuts();

    private void DesktopShortcut_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string path })
            LaunchShortcut(path, message => DesktopShortcutsStatus.Text = message);
    }

    private void DesktopShortcutMenuOpen_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem { Tag: string path })
            LaunchShortcut(path, message => DesktopShortcutsStatus.Text = message);
    }

    private void DesktopShortcutReveal_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuFlyoutItem { Tag: string path } || !File.Exists(path))
        {
            DesktopShortcutsStatus.Text = "Shortcut no longer exists.";
            return;
        }

        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("explorer.exe", $"/select,\"{path}\"")
            {
                UseShellExecute = true
            });
            DesktopShortcutsStatus.Text = $"Showing {Path.GetFileName(path)} in File Explorer.";
        }
        catch (Exception ex)
        {
            DesktopShortcutsStatus.Text = $"Could not open File Explorer: {ex.Message}";
        }
    }

    private void RefreshWindows()
    {
        try
        {
            var ownHandle = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windows = _windowManager.DiscoverWindows(ownHandle);
            _managedWindows.Clear();
            foreach (var window in windows)
                _managedWindows.Add(window);

            WindowManagerStatus.Text = windows.Count == 0
                ? "No other visible application windows were found."
                : $"{windows.Count} visible application windows.";
        }
        catch (Exception ex)
        {
            WindowManagerStatus.Text = $"Could not enumerate windows: {ex.Message}";
        }
    }

    private static bool TryGetWindowHandle(object sender, out long handle)
    {
        if (sender is Button { Tag: long value })
        {
            handle = value;
            return true;
        }

        handle = 0;
        return false;
    }

    private void ActivateWindow_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetWindowHandle(sender, out var handle))
            return;

        WindowManagerStatus.Text = _windowManager.Activate(handle)
            ? "Window activation requested."
            : "Windows did not allow this window to become foreground.";
    }

    private void MinimizeWindow_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetWindowHandle(sender, out var handle))
            return;

        WindowManagerStatus.Text = _windowManager.Minimize(handle)
            ? "Window minimized."
            : "The window is no longer available.";
        RefreshWindows();
    }

    private void CloseWindow_Click(object sender, RoutedEventArgs e)
    {
        if (!TryGetWindowHandle(sender, out var handle))
            return;

        WindowManagerStatus.Text = _windowManager.RequestClose(handle)
            ? "A standard close request was sent. The application decides whether to save or cancel."
            : "Could not send a close request to this window.";
        RefreshWindows();
    }

    private void RefreshWindows_Click(object sender, RoutedEventArgs e) => RefreshWindows();

    private void Windows_Click(object sender, RoutedEventArgs e) =>
        Navigate(ShellSection.Windows, "WINDOW MANAGER", "Every open window, under control.",
            "Inspect visible desktop windows, focus or minimize them, and send standard close requests without terminating processes.");

    private void Desktop_Click(object sender, RoutedEventArgs e) =>
        Navigate(ShellSection.Desktop, "PERSONAL WORKSPACE", "Your desktop, your atmosphere.",
            "A configurable LUMA workspace preview. Wallpaper changes are stored locally and do not modify Windows settings.");

    private void AppSearch_TextChanged(object sender, TextChangedEventArgs e) => ApplyAppFilter();
    private void SpotlightSearch_TextChanged(object sender, TextChangedEventArgs e) => RefreshSpotlightResults();
    private void RefreshApps_Click(object sender, RoutedEventArgs e) => RefreshApplications();

    private void PinApp_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string path })
            PinApplication(path);
    }

    private void LaunchApp_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string path })
            LaunchShortcut(path, message => AppsStatusText.Text = message);
    }

    private void DockApp_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string path })
            LaunchShortcut(path, message => DockEmptyText.Text = message);
    }

    private void SpotlightLaunch_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string path })
        {
            LaunchShortcut(path, message => SpotlightStatusText.Text = message);
            SpotlightOverlay.Visibility = Visibility.Collapsed;
        }
    }

    private void Spotlight_Click(object sender, RoutedEventArgs e)
    {
        if (_allApps.Count == 0)
            RefreshApplications();

        SpotlightOverlay.Visibility = Visibility.Visible;
        SpotlightSearchBox.Focus(FocusState.Programmatic);
        RefreshSpotlightResults();
    }

    private void CloseSpotlight_Click(object sender, RoutedEventArgs e) =>
        SpotlightOverlay.Visibility = Visibility.Collapsed;

    private void Home_Click(object sender, RoutedEventArgs e) =>
        Navigate(ShellSection.Home, "YOUR WINDOWS, REIMAGINED", "A calmer way to work.",
            "LUMA is the foundation for a more coherent Windows desktop. This preview establishes the native shell host and its core navigation.");

    private void Applications_Click(object sender, RoutedEventArgs e) =>
        Navigate(ShellSection.Applications, "APP LIBRARY", "Your apps, in one place.",
            "Installed applications discovered from the Windows Start Menu. Search, pin, and launch your applications.");

    private void Widgets_Click(object sender, RoutedEventArgs e) =>
        Navigate(ShellSection.Widgets, "AT A GLANCE", "Useful information, close by.",
            "A modular widget surface is planned. No background collection or network activity is enabled in this preview.");

    private void Settings_Click(object sender, RoutedEventArgs e) =>
        Navigate(ShellSection.Settings, "PREFERENCES", "Make the desktop yours.",
            "Appearance, behavior, startup, and recovery settings will be introduced incrementally with safe defaults.");
}
