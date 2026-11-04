using Luma.Core.Desktop;
using Xunit;

namespace Luma.Core.Tests;

public sealed class DesktopIconLayoutStateTests
{
    [Fact]
    public void SynchronizeKeepsSavedOrderAndAppendsNewShortcuts()
    {
        var state = new DesktopIconLayoutState();
        state.Restore(["B.lnk", "A.lnk"]);

        state.Synchronize(["A.lnk", "B.lnk", "C.lnk"]);

        Assert.Equal(["B.lnk", "A.lnk", "C.lnk"], state.OrderedPaths);
    }

    [Fact]
    public void SynchronizeRemovesMissingAndDuplicateShortcuts()
    {
        var state = new DesktopIconLayoutState();
        state.Restore(["A.lnk", "B.lnk", "A.LNK"]);

        state.Synchronize(["B.lnk", "C.lnk"]);

        Assert.Equal(["B.lnk", "C.lnk"], state.OrderedPaths);
    }

    [Fact]
    public void MoveChangesOrderAndRejectsOutOfRangeMoves()
    {
        var state = new DesktopIconLayoutState();
        state.Restore(["A.lnk", "B.lnk", "C.lnk"]);

        Assert.True(state.Move("B.lnk", 1));
        Assert.Equal(["A.lnk", "C.lnk", "B.lnk"], state.OrderedPaths);
        Assert.False(state.Move("B.lnk", 1));
        Assert.False(state.Move("missing.lnk", 1));
    }
    [Fact]
    public void MoveToSupportsDirectReordering()
    {
        var state = new DesktopIconLayoutState();
        state.Restore(["A.lnk", "B.lnk", "C.lnk", "D.lnk"]);

        Assert.True(state.MoveTo("A.lnk", 3));
        Assert.Equal(["B.lnk", "C.lnk", "D.lnk", "A.lnk"], state.OrderedPaths);

        Assert.True(state.MoveTo("D.lnk", 0));
        Assert.Equal(["D.lnk", "B.lnk", "C.lnk", "A.lnk"], state.OrderedPaths);
    }

    [Fact]
    public void MoveToRejectsInvalidTargetsAndMissingItems()
    {
        var state = new DesktopIconLayoutState();
        state.Restore(["A.lnk", "B.lnk"]);

        Assert.False(state.MoveTo("A.lnk", -1));
        Assert.False(state.MoveTo("A.lnk", 2));
        Assert.False(state.MoveTo("missing.lnk", 0));
        Assert.False(state.MoveTo("A.lnk", 0));
        Assert.Equal(["A.lnk", "B.lnk"], state.OrderedPaths);
    }

    [Fact]
    public void RestoreRemovesBlankAndCaseInsensitiveDuplicates()
    {
        var state = new DesktopIconLayoutState();

        state.Restore(["A.lnk", "", "a.LNK", "B.lnk", "  "]);

        Assert.Equal(["A.lnk", "B.lnk"], state.OrderedPaths);
    }

    [Fact]
    public void SynchronizeSupportsMixedDesktopFilesFoldersAndShortcuts()
    {
        var state = new DesktopIconLayoutState();
        state.Restore(["C:/Desktop/Editor.lnk", "C:/Desktop/Projects", "C:/Desktop/notes.txt"]);

        state.Synchronize(["C:/Desktop/notes.txt", "C:/Desktop/Editor.lnk", "C:/Desktop/Projects", "C:/Desktop/photo.png"]);

        Assert.Equal(
            ["C:/Desktop/Editor.lnk", "C:/Desktop/Projects", "C:/Desktop/notes.txt", "C:/Desktop/photo.png"],
            state.OrderedPaths);
    }

}
