using System.Collections.ObjectModel;
using Luma.App.Services;
using Luma.Core.Navigation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Luma.App;

public sealed partial class MainWindow : Window
{
    private readonly ShellNavigationState _navigation = new();
    private readonly AppCatalogService _appCatalog = new();
    private readonly DockService _dockService = new();
    private readonly ObservableCollection<AppShortcut> _visibleApps = new();
    private readonly ObservableCollection<AppShortcut> _pinnedApps = new();
    private readonly ObservableCollection<AppShortcut> _spotlightApps = new();
    private IReadOnlyList<AppShortcut> _allApps = Array.Empty<AppShortcut>();
    private bool _updatingCategories;

    public MainWindow()
    {
        InitializeComponent();
        AppsList.ItemsSource = _visibleApps;
        DockItemsControl.ItemsSource = _pinnedApps;
        SpotlightResults.ItemsSource = _spotlightApps;

        foreach (var app in _dockService.Load())
            _pinnedApps.Add(app);

        UpdateDockEmptyState();
    }

    private void Navigate(ShellSection section, string eyebrow, string heading, string description)
    {
        _navigation.NavigateTo(section);
        EyebrowText.Text = eyebrow;
        HeadingText.Text = heading;
        DescriptionText.Text = description;

        var showApplications = section == ShellSection.Applications;
        OverviewPanel.Visibility = showApplications ? Visibility.Collapsed : Visibility.Visible;
        ApplicationsPanel.Visibility = showApplications ? Visibility.Visible : Visibility.Collapsed;

        if (showApplications && _allApps.Count == 0)
            RefreshApplications();
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

    private void UpdateCategoryFilter()\n    {\n        _updatingCategories = true;\n        var selected = AppCategoryFilter.SelectedItem as string;\n        AppCategoryFilter.Items.Clear();\n        AppCategoryFilter.Items.Add("All categories");\n        foreach (var category in _allApps.Select(app => app.Category).Distinct(StringComparer.CurrentCultureIgnoreCase).OrderBy(value => value))\n            AppCategoryFilter.Items.Add(category);\n        AppCategoryFilter.SelectedItem = selected is not null && AppCategoryFilter.Items.Contains(selected) ? selected : "All categories";\n        _updatingCategories = false;\n    }\n\n    private void AppCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)\n    {\n        if (!_updatingCategories) ApplyAppFilter();\n    }\n\n    private void UnpinDockApp_Click(object sender, RoutedEventArgs e)\n    {\n        if (sender is not Button { Tag: string path }) return;\n        var pinned = _pinnedApps.FirstOrDefault(app => string.Equals(app.ShortcutPath, path, StringComparison.OrdinalIgnoreCase));\n        if (pinned is null) return;\n        _pinnedApps.Remove(pinned);\n        PersistDock();\n    }\n\n    private void RefreshSpotlightResults()
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
        var app = _allApps.Concat(_pinnedApps)
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
