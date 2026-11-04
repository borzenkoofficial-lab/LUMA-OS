using System.Text.Json;

namespace Luma.App.Services;

public sealed class DockService
{
    private readonly string _settingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Luma", "settings", "dock.json");

    public IReadOnlyList<AppShortcut> Load()
    {
        try
        {
            if (!File.Exists(_settingsPath))
                return Array.Empty<AppShortcut>();

            var entries = JsonSerializer.Deserialize<List<AppShortcut>>(File.ReadAllText(_settingsPath));
            return entries?
                .Where(entry => !string.IsNullOrWhiteSpace(entry.Name) && !string.IsNullOrWhiteSpace(entry.ShortcutPath))
                .Where(entry => File.Exists(entry.ShortcutPath))
                .ToArray() ?? Array.Empty<AppShortcut>();
        }
        catch (JsonException)
        {
            return Array.Empty<AppShortcut>();
        }
        catch (IOException)
        {
            return Array.Empty<AppShortcut>();
        }
        catch (UnauthorizedAccessException)
        {
            return Array.Empty<AppShortcut>();
        }
    }

    public void Save(IEnumerable<AppShortcut> apps)
    {
        ArgumentNullException.ThrowIfNull(apps);
        var directory = Path.GetDirectoryName(_settingsPath)!;
        Directory.CreateDirectory(directory);
        var payload = apps
            .Where(app => !string.IsNullOrWhiteSpace(app.Name) && !string.IsNullOrWhiteSpace(app.ShortcutPath))
            .GroupBy(app => app.ShortcutPath, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .ToArray();

        var temporaryPath = _settingsPath + ".tmp";
        File.WriteAllText(temporaryPath, JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true }));
        File.Move(temporaryPath, _settingsPath, overwrite: true);
    }
}
