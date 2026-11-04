using System.Diagnostics;

namespace Luma.App.Services;

public sealed record AppShortcut(string Name, string ShortcutPath);

public sealed class AppCatalogService
{
    public IReadOnlyList<AppShortcut> Discover()
    {
        var roots = new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
            Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu)
        };

        var apps = new Dictionary<string, AppShortcut>(StringComparer.OrdinalIgnoreCase);

        foreach (var root in roots.Where(Directory.Exists))
        {
            IEnumerable<string> shortcuts;
            try
            {
                shortcuts = Directory.EnumerateFiles(root, "*.lnk", SearchOption.AllDirectories).ToArray();
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }
            catch (IOException)
            {
                continue;
            }

            foreach (var shortcut in shortcuts)
            {
                var name = Path.GetFileNameWithoutExtension(shortcut);
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                apps.TryAdd(shortcut, new AppShortcut(name, shortcut));
            }
        }

        return apps.Values
            .OrderBy(app => app.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
    }

    public void Launch(AppShortcut app)
    {
        ArgumentNullException.ThrowIfNull(app);

        if (!File.Exists(app.ShortcutPath))
            throw new FileNotFoundException("The application shortcut no longer exists.", app.ShortcutPath);

        Process.Start(new ProcessStartInfo(app.ShortcutPath) { UseShellExecute = true });
    }
}
