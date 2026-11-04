using Luma.Core.Windows;
using Xunit;

namespace Luma.Core.Tests;

public sealed class WindowSnapLayoutTests
{
    [Fact]
    public void CalculatesLeftAndRightHalvesInsideMonitorWorkArea()
    {
        var area = new WorkArea(100, 50, 1920, 1040);

        Assert.Equal(new WindowBounds(100, 50, 960, 1040), WindowSnapLayout.Calculate(area, WindowSnapZone.LeftHalf));
        Assert.Equal(new WindowBounds(1060, 50, 960, 1040), WindowSnapLayout.Calculate(area, WindowSnapZone.RightHalf));
    }

    [Fact]
    public void SplitsOddDimensionsWithoutGapsOrOverlap()
    {
        var area = new WorkArea(-1920, 20, 1919, 1001);

        var left = WindowSnapLayout.Calculate(area, WindowSnapZone.LeftHalf);
        var right = WindowSnapLayout.Calculate(area, WindowSnapZone.RightHalf);
        var topLeft = WindowSnapLayout.Calculate(area, WindowSnapZone.TopLeft);
        var bottomRight = WindowSnapLayout.Calculate(area, WindowSnapZone.BottomRight);

        Assert.Equal(960, left.Width);
        Assert.Equal(959, right.Width);
        Assert.Equal(1919, left.Width + right.Width);
        Assert.Equal(501, topLeft.Height);
        Assert.Equal(500, bottomRight.Height);
        Assert.Equal(-961, right.Left);
        Assert.Equal(521, bottomRight.Top);
    }

    [Theory]
    [InlineData(WindowSnapZone.TopLeft)]
    [InlineData(WindowSnapZone.TopRight)]
    [InlineData(WindowSnapZone.BottomLeft)]
    [InlineData(WindowSnapZone.BottomRight)]
    public void CalculatesAllQuadrants(WindowSnapZone zone)
    {
        var bounds = WindowSnapLayout.Calculate(new WorkArea(0, 0, 1600, 900), zone);

        Assert.Equal(800, bounds.Width);
        Assert.Equal(450, bounds.Height);
    }

    [Fact]
    public void RejectsInvalidWorkArea()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            WindowSnapLayout.Calculate(new WorkArea(0, 0, 0, 900), WindowSnapZone.LeftHalf));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            WindowSnapLayout.Calculate(new WorkArea(0, 0, 1600, -1), WindowSnapZone.RightHalf));
    }

    [Fact]
    public void RejectsUndefinedSnapZone()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            WindowSnapLayout.Calculate(new WorkArea(0, 0, 100, 100), (WindowSnapZone)999));
    }
}
