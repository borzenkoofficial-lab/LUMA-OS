using System.Diagnostics;

namespace Luma.App.Services;

public sealed record AppShortcut(string Name, string ShortcutPath, string Category = "Other");

public enum DesktopItemKind
{
    Folder,
    Shortcut,
    File
}

public sealed record DesktopItem(string Name, string FullPath, DesktopItemKind Kind)
{
    public string KindLabel => Kind switch
    {
        DesktopItemKind.Folder => "FOLDER",
        DesktopItemKind.Shortcut => "APP SHORTCUT",
        _ => "FILE"
    };
}

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

    public IReadOnlyList<DesktopItem> DiscoverDesktopItems()
    {
        var roots = new[]
        {
            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
            Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory)
        };

        var items = new Dictionary<string, DesktopItem>(StringComparer.OrdinalIgnoreCase);
        foreach (var root in roots.Where(path => !string.IsNullOrWhiteSpace(path) && Directory.Exists(path)))
        {
            try
            {
                foreach (var path in Directory.EnumerateFileSystemEntries(root, "*", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        var attributes = File.GetAttributes(path);
                        if ((attributes & (FileAttributes.Hidden | FileAttributes.System)) != 0)
                            continue;

                        var isDirectory = (attributes & FileAttributes.Directory) != 0;
                        var kind = isDirectory
                            ? DesktopItemKind.Folder
                            : Path.GetExtension(path).Equals(".lnk", StringComparison.OrdinalIgnoreCase)
                                ? DesktopItemKind.Shortcut
                                : DesktopItemKind.File;
                        var name = isDirectory || kind == DesktopItemKind.File
                            ? Path.GetFileName(path)
                            : Path.GetFileNameWithoutExtension(path);

                        if (!string.IsNullOrWhiteSpace(name))
                            items.TryAdd(path, new DesktopItem(name, path, kind));
                    }
                    catch (UnauthorizedAccessException) { }
                    catch (IOException) { }
                }
            }
            catch (UnauthorizedAccessException) { }
            catch (IOException) { }
        }

        return items.Values
            .OrderByDescending(item => item.Kind == DesktopItemKind.Folder)
            .ThenBy(item => item.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
    }

    public void OpenDesktopItem(DesktopItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        var exists = item.Kind == DesktopItemKind.Folder
            ? Directory.Exists(item.FullPath)
            : File.Exists(item.FullPath);
        if (!exists)
            throw new FileNotFoundException("The desktop item no longer exists.", item.FullPath);

        Process.Start(new ProcessStartInfo(item.FullPath) { UseShellExecute = true });
    }

    public void Launch(AppShortcut app)
    {
        ArgumentNullException.ThrowIfNull(app);
        if (!File.Exists(app.ShortcutPath))
            throw new FileNotFoundException("The application shortcut no longer exists.", app.ShortcutPath);

        Process.Start(new ProcessStartInfo(app.ShortcutPath) { UseShellExecute = true });
    }
}
