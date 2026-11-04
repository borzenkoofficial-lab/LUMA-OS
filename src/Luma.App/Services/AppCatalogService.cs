using System.Diagnostics;

namespace Luma.App.Services;

public sealed record AppShortcut(string Name, string ShortcutPath, string Category = "Other");

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
            var pending = new Stack<string>();
            pending.Push(root);

            while (pending.Count > 0)
            {
                var directory = pending.Pop();
                try
                {
                    foreach (var shortcut in Directory.EnumerateFiles(directory, "*.lnk", SearchOption.TopDirectoryOnly))
                    {
                        var name = Path.GetFileNameWithoutExtension(shortcut);
                        if (string.IsNullOrWhiteSpace(name))
                            continue;

                        var relative = Path.GetRelativePath(root, directory);
                        var category = relative == "." ? "Other" : relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)[0];
                        apps.TryAdd(shortcut, new AppShortcut(name, shortcut, category));
                    }

                    foreach (var child in Directory.EnumerateDirectories(directory, "*", SearchOption.TopDirectoryOnly))
                        pending.Push(child);
                }
                catch (UnauthorizedAccessException) { }
                catch (IOException) { }
            }
        }

        return apps.Values
            .OrderBy(app => app.Category, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(app => app.Name, StringComparer.CurrentCultureIgnoreCase)
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
