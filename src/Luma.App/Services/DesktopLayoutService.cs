using System.Text.Json;

namespace Luma.App.Services;

public sealed class DesktopLayoutService
{
    private readonly string _layoutPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Luma", "settings", "desktop-layout.json");

    public IReadOnlyList<string> Load()
    {
        try
        {
            if (!File.Exists(_layoutPath))
                return Array.Empty<string>();

            var paths = JsonSerializer.Deserialize<string[]>(File.ReadAllText(_layoutPath));
            return Normalize(paths ?? Array.Empty<string>());
        }
        catch (JsonException) { return Array.Empty<string>(); }
        catch (IOException) { return Array.Empty<string>(); }
        catch (UnauthorizedAccessException) { return Array.Empty<string>(); }
    }

    public void Save(IEnumerable<string> orderedPaths)
    {
        ArgumentNullException.ThrowIfNull(orderedPaths);

        var normalized = Normalize(orderedPaths);
        var directory = Path.GetDirectoryName(_layoutPath)!;
        Directory.CreateDirectory(directory);
        var temporaryPath = _layoutPath + ".tmp";

        try
        {
            File.WriteAllText(temporaryPath, JsonSerializer.Serialize(
                normalized, new JsonSerializerOptions { WriteIndented = true }));
            File.Move(temporaryPath, _layoutPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
                File.Delete(temporaryPath);
        }
    }

    private static string[] Normalize(IEnumerable<string> paths) => paths
        .Where(path => !string.IsNullOrWhiteSpace(path))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();
}
