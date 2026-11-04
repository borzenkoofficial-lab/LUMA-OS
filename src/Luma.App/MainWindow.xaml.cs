using Luma.Core.Navigation;
using Microsoft.UI.Xaml;

namespace Luma.App;

public sealed partial class MainWindow : Window
{
    private readonly ShellNavigationState _navigation = new();

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Navigate(ShellSection section, string eyebrow, string heading, string description)
    {
        _navigation.NavigateTo(section);
        EyebrowText.Text = eyebrow;
        HeadingText.Text = heading;
        DescriptionText.Text = description;
    }

    private void Home_Click(object sender, RoutedEventArgs e) =>
        Navigate(ShellSection.Home, "YOUR WINDOWS, REIMAGINED", "A calmer way to work.",
            "LUMA is the foundation for a more coherent Windows desktop. This preview establishes the native shell host and its core navigation.");

    private void Applications_Click(object sender, RoutedEventArgs e) =>
        Navigate(ShellSection.Applications, "APP LIBRARY", "Your apps, in one place.",
            "The application catalog and Launchpad experience are planned for the next foundation milestone.");

    private void Widgets_Click(object sender, RoutedEventArgs e) =>
        Navigate(ShellSection.Widgets, "AT A GLANCE", "Useful information, close by.",
            "A modular widget surface is planned. No background collection or network activity is enabled in this preview.");

    private void Settings_Click(object sender, RoutedEventArgs e) =>
        Navigate(ShellSection.Settings, "PREFERENCES", "Make the desktop yours.",
            "Appearance, behavior, startup, and recovery settings will be introduced incrementally with safe defaults.");
}
