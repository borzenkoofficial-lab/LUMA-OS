namespace Luma.Core.Desktop;

public sealed class DesktopIconLayoutState
{
    private readonly List<string> _orderedPaths = new();

    public IReadOnlyList<string> OrderedPaths => _orderedPaths.AsReadOnly();

    public void Synchronize(IEnumerable<string> discoveredPaths)
    {
        ArgumentNullException.ThrowIfNull(discoveredPaths);

        var discovered = discoveredPaths
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var available = new HashSet<string>(discovered, StringComparer.OrdinalIgnoreCase);
        var retained = _orderedPaths.Where(available.Contains).ToList();
        var retainedSet = new HashSet<string>(retained, StringComparer.OrdinalIgnoreCase);

        foreach (var path in discovered)
        {
            if (retainedSet.Add(path))
                retained.Add(path);
        }

        _orderedPaths.Clear();
        _orderedPaths.AddRange(retained);
    }

    public void Restore(IEnumerable<string> savedPaths)
    {
        ArgumentNullException.ThrowIfNull(savedPaths);
        _orderedPaths.Clear();
        _orderedPaths.AddRange(savedPaths
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.OrdinalIgnoreCase));
    }

    public bool MoveTo(string path, int targetIndex)
    {
        if (string.IsNullOrWhiteSpace(path) || targetIndex < 0 || targetIndex >= _orderedPaths.Count)
            return false;

        var currentIndex = _orderedPaths.FindIndex(item =>
            string.Equals(item, path, StringComparison.OrdinalIgnoreCase));

        if (currentIndex < 0 || currentIndex == targetIndex)
            return false;

        var item = _orderedPaths[currentIndex];
        _orderedPaths.RemoveAt(currentIndex);
        _orderedPaths.Insert(targetIndex, item);
        return true;
    }

    public bool Move(string path, int offset)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        var currentIndex = _orderedPaths.FindIndex(item =>
            string.Equals(item, path, StringComparison.OrdinalIgnoreCase));
        var targetIndex = currentIndex + offset;

        if (currentIndex < 0 || offset == 0 || targetIndex < 0 || targetIndex >= _orderedPaths.Count)
            return false;

        (_orderedPaths[currentIndex], _orderedPaths[targetIndex]) =
            (_orderedPaths[targetIndex], _orderedPaths[currentIndex]);
        return true;
    }
}
