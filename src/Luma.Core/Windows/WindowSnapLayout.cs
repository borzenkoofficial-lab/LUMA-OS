namespace Luma.Core.Windows;

public enum WindowSnapZone
{
    LeftHalf,
    RightHalf,
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight
}

public readonly record struct WorkArea(int Left, int Top, int Width, int Height);

public readonly record struct WindowBounds(int Left, int Top, int Width, int Height);

public static class WindowSnapLayout
{
    public static WindowBounds Calculate(WorkArea area, WindowSnapZone zone)
    {
        if (area.Width <= 0)
            throw new ArgumentOutOfRangeException(nameof(area), "Work area width must be positive.");
        if (area.Height <= 0)
            throw new ArgumentOutOfRangeException(nameof(area), "Work area height must be positive.");

        var leftWidth = (area.Width + 1) / 2;
        var rightWidth = area.Width - leftWidth;
        var topHeight = (area.Height + 1) / 2;
        var bottomHeight = area.Height - topHeight;
        var right = area.Left + leftWidth;
        var bottom = area.Top + topHeight;

        return zone switch
        {
            WindowSnapZone.LeftHalf => new WindowBounds(area.Left, area.Top, area.Width / 2 + area.Width % 2, area.Height),
            WindowSnapZone.RightHalf => new WindowBounds(right, area.Top, rightWidth, area.Height),
            WindowSnapZone.TopLeft => new WindowBounds(area.Left, area.Top, leftWidth, topHeight),
            WindowSnapZone.TopRight => new WindowBounds(right, area.Top, rightWidth, topHeight),
            WindowSnapZone.BottomLeft => new WindowBounds(area.Left, bottom, leftWidth, bottomHeight),
            WindowSnapZone.BottomRight => new WindowBounds(right, bottom, rightWidth, bottomHeight),
            _ => throw new ArgumentOutOfRangeException(nameof(zone), zone, "Unsupported snap zone.")
        };
    }
}
