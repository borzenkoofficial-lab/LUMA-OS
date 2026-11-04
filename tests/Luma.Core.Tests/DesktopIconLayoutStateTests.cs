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
}
