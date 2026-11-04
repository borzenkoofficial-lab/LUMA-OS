using Luma.Core.Files;
using Xunit;

namespace Luma.Core.Tests;

public sealed class FileBrowserPathStateTests
{
    [Fact]
    public void StartsWithoutASelectedFolder()
    {
        var state = new FileBrowserPathState();

        Assert.Null(state.CurrentPath);
        Assert.Null(state.ParentPath);
    }

    [Fact]
    public void NavigatesToCanonicalAbsolutePath()
    {
        var state = new FileBrowserPathState();
        var expected = Path.GetFullPath(".");

        Assert.True(state.TryNavigate(Path.Combine(".", "temp", "..")));
        Assert.Equal(expected, state.CurrentPath);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void RejectsBlankPathWithoutChangingCurrentFolder(string? path)
    {
        var state = new FileBrowserPathState();
        state.TryNavigate(".");

        var current = state.CurrentPath;

        Assert.False(state.TryNavigate(path));
        Assert.Equal(current, state.CurrentPath);
    }

    [Fact]
    public void ResolvesParentDirectory()
    {
        var state = new FileBrowserPathState();
        var path = Path.GetFullPath(Path.Combine(".", "child"));

        Assert.True(state.TryNavigate(path));
        Assert.Equal(Directory.GetParent(path)?.FullName, state.ParentPath);
    }
}
