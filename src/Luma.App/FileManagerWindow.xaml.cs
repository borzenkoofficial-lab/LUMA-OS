using System.Collections.ObjectModel;
using Luma.App.Services;
using Luma.Core.Files;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.Graphics;

namespace Luma.App;

public sealed partial class FileManagerWindow : Window
{
    private readonly FileBrowserService _browser = new();
    private readonly FileBrowserPathState _pathState = new();
    private readonly ObservableCollection<FileSystemEntry> _visibleEntries = new();
    private IReadOnlyList<FileSystemEntry> _allEntries = Array.Empty<FileSystemEntry>();
    private readonly Stack<string> _history = new();
    private string? _currentPath;
    private bool _showingDrives;

    public FileManagerWindow()
    {
        InitializeComponent();
        EntriesList.ItemsSource = _visibleEntries;
        ConfigureWindow();
        NavigateTo(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), addHistory: false);
    }

    private void ConfigureWindow()
    {
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);

        if (appWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.SetBorderAndTitleBar(false, false);
            presenter.IsResizable = true;
            presenter.IsMaximizable = true;
            presenter.IsMinimizable = true;
        }

        var area = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary).WorkArea;
        const int width = 1080;
        const int height = 720;
        var x = area.X + Math.Max(0, (area.Width - width) / 2);
        var y = area.Y + Math.Max(0, (area.Height - height) / 2);
        appWindow.MoveAndResize(new RectInt32(x, y, Math.Min(width, area.Width), Math.Min(height, area.Height)));
    }

    private void NavigateTo(string path, bool addHistory = true)
    {
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
        {
            StatusText.Text = "The folder does not exist or is not accessible.";
            return;
        }

        if (!_pathState.TryNavigate(path))
        {
            StatusText.Text = "Invalid folder path.";
            return;
        }

        var fullPath = _pathState.CurrentPath!;
        try
        {
            var entries = _browser.Browse(fullPath);
            if (addHistory && !_showingDrives && !string.IsNullOrWhiteSpace(_currentPath) &&
                !string.Equals(_currentPath, fullPath, StringComparison.OrdinalIgnoreCase))
            {
                _history.Push(_currentPath);
            }

            _currentPath = fullPath;
            _showingDrives = false;
            _allEntries = entries;
            PathBox.Text = fullPath;
            FolderTitle.Text = new DirectoryInfo(fullPath).Name;
            ApplySearch();
            StatusText.Text = $"{entries.Count} items";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Cannot open folder: {ex.Message}";
        }
    }

    private void ShowDrives()
    {
        if (!_showingDrives && !string.IsNullOrWhiteSpace(_currentPath))
            _history.Push(_currentPath);
        _currentPath = null;
        _showingDrives = true;
        _allEntries = DriveInfo.GetDrives()
            .Where(drive =>
            {
                try { return drive.IsReady; }
                catch { return false; }
            })
            .Select(drive => new FileSystemEntry(
                drive.Name,
                drive.RootDirectory.FullName,
                true,
                $"{drive.DriveType} • {FormatDriveSpace(drive)}"))
            .ToArray();

        PathBox.Text = "This PC";
        FolderTitle.Text = "This PC";
        ApplySearch();
        StatusText.Text = $"{_allEntries.Count} available drives";
    }

    private static string FormatDriveSpace(DriveInfo drive)
    {
        try
        {
            var free = drive.AvailableFreeSpace / 1024d / 1024 / 1024;
            var total = drive.TotalSize / 1024d / 1024 / 1024;
            return $"{free:0.#} GB free of {total:0.#} GB";
        }
        catch
        {
            return "Space unavailable";
        }
    }

    private void ApplySearch()
    {
        var query = SearchBox.Text?.Trim() ?? string.Empty;
        var matches = string.IsNullOrWhiteSpace(query)
            ? _allEntries
            : _allEntries.Where(entry =>
                entry.Name.Contains(query, StringComparison.CurrentCultureIgnoreCase)).ToArray();

        _visibleEntries.Clear();
        foreach (var entry in matches)
            _visibleEntries.Add(entry);
    }

    private void OpenKnownFolder(Environment.SpecialFolder folder)
    {
        var path = Environment.GetFolderPath(folder);
        if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
            NavigateTo(path);
        else
            StatusText.Text = "This folder is not available on this PC.";
    }

    private void Home_Click(object sender, RoutedEventArgs e) =>
        NavigateTo(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

    private void Desktop_Click(object sender, RoutedEventArgs e) =>
        OpenKnownFolder(Environment.SpecialFolder.DesktopDirectory);

    private void Documents_Click(object sender, RoutedEventArgs e) =>
        OpenKnownFolder(Environment.SpecialFolder.MyDocuments);

    private void Pictures_Click(object sender, RoutedEventArgs e) =>
        OpenKnownFolder(Environment.SpecialFolder.MyPictures);

    private void Downloads_Click(object sender, RoutedEventArgs e)
    {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        if (Directory.Exists(path))
            NavigateTo(path);
        else
            StatusText.Text = "Downloads folder not found.";
    }

    private void ThisPc_Click(object sender, RoutedEventArgs e) => ShowDrives();

    private void Up_Click(object sender, RoutedEventArgs e)
    {
        if (_showingDrives)
            return;
        if (_pathState.ParentPath is { } parent)
            NavigateTo(parent);
        else
            ShowDrives();
    }

    private void Back_Click(object sender, RoutedEventArgs e)
    {
        if (_history.Count == 0)
        {
            StatusText.Text = "No previous folder.";
            return;
        }

        var previous = _history.Pop();
        NavigateTo(previous, addHistory: false);
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => ApplySearch();

    private void EntriesList_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is not FileSystemEntry entry)
            return;

        if (entry.IsDirectory)
        {
            NavigateTo(entry.FullPath);
            return;
        }

        try
        {
            _browser.OpenFile(entry);
            StatusText.Text = $"Opened {entry.Name}";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Could not open file: {ex.Message}";
        }
    }

    private void PathBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key != VirtualKey.Enter)
            return;

        if (string.Equals(PathBox.Text, "This PC", StringComparison.OrdinalIgnoreCase))
            ShowDrives();
        else
            NavigateTo(PathBox.Text);
    }

    private async void ChooseFolder_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var picker = new FolderPicker { SuggestedStartLocation = PickerLocationId.ComputerFolder };
            picker.FileTypeFilter.Add("*");
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
            var folder = await picker.PickSingleFolderAsync();
            if (folder is not null)
                NavigateTo(folder.Path);
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Could not choose folder: {ex.Message}";
        }
    }
}
