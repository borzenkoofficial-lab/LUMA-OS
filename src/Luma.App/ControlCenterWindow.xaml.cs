using System.Diagnostics;
using System.Net.NetworkInformation;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Graphics;

namespace Luma.App;

public sealed partial class ControlCenterWindow : Window
{
    public ControlCenterWindow()
    {
        InitializeComponent();
        ConfigureWindow();
        NetworkStatus.Text = NetworkInterface.GetIsNetworkAvailable()
            ? "Network available"
            : "No network connection";
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

        const int width = 350;
        const int height = 390;
        var workArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary).WorkArea;
        var x = workArea.X + Math.Max(0, workArea.Width - width - 16);
        var y = workArea.Y + 44;
        appWindow.MoveAndResize(new RectInt32(x, y, width, height));
    }

    private static void OpenSettings(string uri)
    {
        try
        {
            Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
        }
        catch
        {
            // The panel remains available if Windows Settings cannot be opened.
        }
    }

    private void Network_Click(object sender, RoutedEventArgs e) => OpenSettings("ms-settings:network");
    private void Bluetooth_Click(object sender, RoutedEventArgs e) => OpenSettings("ms-settings:bluetooth");
    private void Display_Click(object sender, RoutedEventArgs e) => OpenSettings("ms-settings:display");
    private void Sound_Click(object sender, RoutedEventArgs e) => OpenSettings("ms-settings:sound");
    private void Close_Click(object sender, RoutedEventArgs e) => Close();
}
