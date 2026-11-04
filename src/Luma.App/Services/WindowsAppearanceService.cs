using System.Runtime.InteropServices;
using System.Text.Json;
using Microsoft.Win32;

namespace Luma.App.Services;

public sealed class WindowsAppearanceService
{
    private const string PersonalizeKey = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
    private const string ExplorerAdvancedKey = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
    private const uint SpiSetDesktopWallpaper = 0x0014;
    private const uint SpifUpdateIniFile = 0x01;
    private const uint SpifSendChange = 0x02;
    private const uint WmSettingChange = 0x001A;
    private const uint SmtoAbortIfHung = 0x0002;
    private const int HwndBroadcast = 0xffff;

    private readonly string _backupPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Luma", "backup", "windows-appearance.json");

    public void Apply(string wallpaperPath)
    {
        if (string.IsNullOrWhiteSpace(wallpaperPath) || !File.Exists(wallpaperPath))
            throw new FileNotFoundException("Choose an existing wallpaper image first.", wallpaperPath);

        var personalize = Registry.CurrentUser.OpenSubKey(PersonalizeKey, writable: true)
            ?? Registry.CurrentUser.CreateSubKey(PersonalizeKey, writable: true);
        var advanced = Registry.CurrentUser.OpenSubKey(ExplorerAdvancedKey, writable: true)
            ?? Registry.CurrentUser.CreateSubKey(ExplorerAdvancedKey, writable: true);

        var backup = LoadBackup() ?? new AppearanceBackup(
            ReadDword(personalize, "AppsUseLightTheme"),
            ReadDword(personalize, "SystemUsesLightTheme"),
            ReadDword(advanced, "TaskbarAl"),
            ReadWallpaper(),
            wallpaperPath);

        backup = backup with { AppliedWallpaper = wallpaperPath };
        SaveBackup(backup);

        personalize.SetValue("AppsUseLightTheme", 0, RegistryValueKind.DWord);
        personalize.SetValue("SystemUsesLightTheme", 0, RegistryValueKind.DWord);
        advanced.SetValue("TaskbarAl", 1, RegistryValueKind.DWord);

        if (!SystemParametersInfo(SpiSetDesktopWallpaper, 0, wallpaperPath, SpifUpdateIniFile | SpifSendChange))
            throw new InvalidOperationException("Windows rejected the wallpaper change.");

        NotifySettingsChanged();
    }

    public bool Restore()
    {
        var backup = LoadBackup();
        if (backup is null)
            return false;

        using var personalize = Registry.CurrentUser.OpenSubKey(PersonalizeKey, writable: true);
        using var advanced = Registry.CurrentUser.OpenSubKey(ExplorerAdvancedKey, writable: true);

        RestoreDword(personalize, "AppsUseLightTheme", 0, backup.AppsUseLightTheme);
        RestoreDword(personalize, "SystemUsesLightTheme", 0, backup.SystemUsesLightTheme);
        RestoreDword(advanced, "TaskbarAl", 1, backup.TaskbarAlignment);

        var currentWallpaper = ReadWallpaper();
        if (string.Equals(currentWallpaper, backup.AppliedWallpaper, StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(backup.OriginalWallpaper) &&
            File.Exists(backup.OriginalWallpaper))
        {
            SystemParametersInfo(SpiSetDesktopWallpaper, 0, backup.OriginalWallpaper, SpifUpdateIniFile | SpifSendChange);
        }

        File.Delete(_backupPath);
        NotifySettingsChanged();
        return true;
    }

    private AppearanceBackup? LoadBackup()
    {
        try
        {
            return File.Exists(_backupPath)
                ? JsonSerializer.Deserialize<AppearanceBackup>(File.ReadAllText(_backupPath))
                : null;
        }
        catch (JsonException) { return null; }
        catch (IOException) { return null; }
        catch (UnauthorizedAccessException) { return null; }
    }

    private void SaveBackup(AppearanceBackup backup)
    {
        var directory = Path.GetDirectoryName(_backupPath)!;
        Directory.CreateDirectory(directory);
        var temporaryPath = _backupPath + ".tmp";
        File.WriteAllText(temporaryPath, JsonSerializer.Serialize(backup, new JsonSerializerOptions { WriteIndented = true }));
        File.Move(temporaryPath, _backupPath, overwrite: true);
    }

    private static int? ReadDword(RegistryKey key, string name)
    {
        var value = key.GetValue(name);
        return value is int number ? number : null;
    }

    private static void RestoreDword(RegistryKey? key, string name, int appliedValue, int? originalValue)
    {
        if (key is null || ReadDword(key, name) != appliedValue)
            return;

        if (originalValue.HasValue)
            key.SetValue(name, originalValue.Value, RegistryValueKind.DWord);
        else
            key.DeleteValue(name, throwOnMissingValue: false);
    }

    private static string? ReadWallpaper()
    {
        using var key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop");
        return key?.GetValue("WallPaper") as string;
    }

    private static void NotifySettingsChanged()
    {
        SendMessageTimeout(new IntPtr(HwndBroadcast), WmSettingChange, IntPtr.Zero,
            "ImmersiveColorSet", SmtoAbortIfHung, 1000, out _);
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool SystemParametersInfo(uint action, uint parameter, string value, uint flags);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr SendMessageTimeout(IntPtr window, uint message, IntPtr wParam,
        string lParam, uint flags, uint timeout, out IntPtr result);

    private sealed record AppearanceBackup(
        int? AppsUseLightTheme,
        int? SystemUsesLightTheme,
        int? TaskbarAlignment,
        string? OriginalWallpaper,
        string AppliedWallpaper);
}
