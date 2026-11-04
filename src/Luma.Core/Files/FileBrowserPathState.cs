namespace Luma.Core.Files;

public sealed class FileBrowserPathState
{
    public string? CurrentPath { get; private set; }

    public string? ParentPath => CurrentPath is null
        ? null
        : Directory.GetParent(CurrentPath)?.FullName;

    public bool TryNavigate(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        try
        {
            CurrentPath = Path.GetFullPath(path);
            return true;
        }
        catch (ArgumentException) { return false; }
        catch (NotSupportedException) { return false; }
        catch (PathTooLongException) { return false; }
    }
}
