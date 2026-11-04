using System.Text.Json;

namespace Luma.App.Services;

public sealed class DesktopPreferencesService
{
    private readonly string _settingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Luma", "settings", "desktop.json");

    public string? LoadWallpaperPath()
    {
        try
        {
            if (!File.Exists(_settingsPath))
                return null;

            var settings = JsonSerializer.Deserialize<DesktopPreferences>(File.ReadAllText(_settingsPath));
            return !string.IsNullOrWhiteSpace(settings?.WallpaperPath) && File.Exists(settings.WallpaperPath)
                ? settings.WallpaperPath
                : null;
        }
        catch (JsonException) { return null; }
        catch (IOException) { return null; }
        catch (UnauthorizedAccessException) { return null; }
    }

    public void SaveWallpaperPath(string wallpaperPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(wallpaperPath);
        if (!File.Exists(wallpaperPath))
            throw new FileNotFoundException("The selected wallpaper file does not exist.", wallpaperPath);

        var directory = Path.GetDirectoryName(_settingsPath)!;
        Directory.CreateDirectory(directory);
        var temporaryPath = _settingsPath + ".tmp";
        var settings = new DesktopPreferences(wallpaperPath);
        File.WriteAllText(temporaryPath, JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
        File.Move(temporaryPath, _settingsPath, overwrite: true);
    }

    public void ClearWallpaperPath()
    {
        try
        {
            if (File.Exists(_settingsPath))
                File.Delete(_settingsPath);
        }
        catch (IOException) { throw; }
        catch (UnauthorizedAccessException) { throw; }
    }

    private sealed record DesktopPreferences(string WallpaperPath);
}
