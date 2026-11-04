using System.Diagnostics;
using System.Globalization;

namespace Luma.App.Services;

public sealed record FileSystemEntry(string Name, string FullPath, bool IsDirectory, string Details)
{
    public string Kind => IsDirectory ? "FOLDER" : "FILE";
}

public sealed class FileBrowserService
{
    public IReadOnlyList<FileSystemEntry> Browse(string directoryPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directoryPath);
        var fullPath = Path.GetFullPath(directoryPath);
        if (!Directory.Exists(fullPath))
            throw new DirectoryNotFoundException($"Folder does not exist: {fullPath}");

        var entries = new List<FileSystemEntry>();

        foreach (var path in Directory.EnumerateDirectories(fullPath))
        {
            try
            {
                var info = new DirectoryInfo(path);
                entries.Add(new FileSystemEntry(info.Name, info.FullName, true,
                    $"Modified {info.LastWriteTime.ToString("g", CultureInfo.CurrentCulture)}"));
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        foreach (var path in Directory.EnumerateFiles(fullPath))
        {
            try
            {
                var info = new FileInfo(path);
                entries.Add(new FileSystemEntry(info.Name, info.FullName, false,
                    $"{FormatSize(info.Length)} • Modified {info.LastWriteTime.ToString("g", CultureInfo.CurrentCulture)}"));
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }

        return entries
            .OrderByDescending(entry => entry.IsDirectory)
            .ThenBy(entry => entry.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
    }

    public void OpenFile(FileSystemEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        if (entry.IsDirectory)
            throw new InvalidOperationException("Directories must be opened through folder navigation.");
        if (!File.Exists(entry.FullPath))
            throw new FileNotFoundException("The file no longer exists.", entry.FullPath);

        Process.Start(new ProcessStartInfo(entry.FullPath) { UseShellExecute = true });
    }

    private static string FormatSize(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB", "TB"];
        var size = (double)bytes;
        var unit = 0;
        while (size >= 1024 && unit < units.Length - 1)
        {
            size /= 1024;
            unit++;
        }

        return $"{size:0.#} {units[unit]}";
    }
}
