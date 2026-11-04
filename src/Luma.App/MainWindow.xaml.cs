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
    private readonly ObservableCollection<AppShortcut> _visibleApps = new();
    private IReadOnlyList<AppShortcut> _allApps = Array.Empty<AppShortcut>();

    public MainWindow()
    {
        InitializeComponent();
        AppsList.ItemsSource = _visibleApps;
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
            ApplyAppFilter();
            AppsStatusText.Text = _allApps.Count == 0
                ? "No Start Menu shortcuts found. Packaged apps without shortcuts are not listed yet."
                : $"Found {_allApps.Count} Start Menu shortcuts.";
        }
        catch (Exception ex)
        {
            AppsStatusText.Text = $"Could not read the Start Menu: {ex.Message}";
        }
    }

    private void ApplyAppFilter()
    {
        var query = AppSearchBox.Text?.Trim() ?? string.Empty;
        var matches = string.IsNullOrEmpty(query)
            ? _allApps
            : _allApps.Where(app => app.Name.Contains(query, StringComparison.CurrentCultureIgnoreCase)).ToArray();

        _visibleApps.Clear();
        foreach (var app in matches)
            _visibleApps.Add(app);

        if (_allApps.Count > 0)
            AppsStatusText.Text = $"{_visibleApps.Count} of {_allApps.Count} applications";
    }

    private void AppSearch_TextChanged(object sender, TextChangedEventArgs e) => ApplyAppFilter();

    private void RefreshApps_Click(object sender, RoutedEventArgs e) => RefreshApplications();

    private void LaunchApp_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: string shortcutPath })
            return;

        var app = _allApps.FirstOrDefault(item => string.Equals(item.ShortcutPath, shortcutPath, StringComparison.OrdinalIgnoreCase));
        if (app is null)
        {
            AppsStatusText.Text = "This shortcut is no longer in the application catalog. Refresh and try again.";
            return;
        }

        try
        {
            _appCatalog.Launch(app);
            AppsStatusText.Text = $"Launched {app.Name}.";
        }
        catch (Exception ex)
        {
            AppsStatusText.Text = $"Could not launch {app.Name}: {ex.Message}";
        }
    }

    private void Home_Click(object sender, RoutedEventArgs e) =>
        Navigate(ShellSection.Home, "YOUR WINDOWS, REIMAGINED", "A calmer way to work.",
            "LUMA is the foundation for a more coherent Windows desktop. This preview establishes the native shell host and its core navigation.");

    private void Applications_Click(object sender, RoutedEventArgs e) =>
        Navigate(ShellSection.Applications, "APP LIBRARY", "Your apps, in one place.",
            "Installed applications discovered from the Windows Start Menu. Select Open to launch an application.");

    private void Widgets_Click(object sender, RoutedEventArgs e) =>
        Navigate(ShellSection.Widgets, "AT A GLANCE", "Useful information, close by.",
            "A modular widget surface is planned. No background collection or network activity is enabled in this preview.");

    private void Settings_Click(object sender, RoutedEventArgs e) =>
        Navigate(ShellSection.Settings, "PREFERENCES", "Make the desktop yours.",
            "Appearance, behavior, startup, and recovery settings will be introduced incrementally with safe defaults.");
}
