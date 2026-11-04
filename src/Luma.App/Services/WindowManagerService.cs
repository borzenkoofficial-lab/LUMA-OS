using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Luma.App.Services;

public sealed record ManagedWindow(long Handle, string Title, string ProcessName, bool IsMinimized);

public sealed class WindowManagerService
{
    private const int SwRestore = 9;
    private const int SwMinimize = 6;
    private const uint WmClose = 0x0010;

    public IReadOnlyList<ManagedWindow> DiscoverWindows(IntPtr excludedWindow)
    {
        var windows = new List<ManagedWindow>();

        EnumWindows((handle, _) =>
        {
            if (handle == excludedWindow || !IsWindowVisible(handle))
                return true;

            var titleLength = GetWindowTextLength(handle);
            if (titleLength <= 0)
                return true;

            var titleBuilder = new StringBuilder(titleLength + 1);
            if (GetWindowText(handle, titleBuilder, titleBuilder.Capacity) <= 0)
                return true;

            GetWindowThreadProcessId(handle, out var processId);
            var processName = ResolveProcessName(processId);

            windows.Add(new ManagedWindow(
                handle.ToInt64(),
                titleBuilder.ToString().Trim(),
                processName,
                IsIconic(handle)));

            return true;
        }, IntPtr.Zero);

        return windows
            .Where(window => !string.IsNullOrWhiteSpace(window.Title))
            .OrderBy(window => window.Title, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
    }

    public bool Activate(long handle)
    {
        var window = new IntPtr(handle);
        if (!IsWindow(window))
            return false;

        ShowWindow(window, SwRestore);
        return SetForegroundWindow(window);
    }

    public bool Minimize(long handle)
    {
        var window = new IntPtr(handle);
        if (!IsWindow(window))
            return false;

        ShowWindow(window, SwMinimize);
        return IsWindow(window);
    }

    public bool RequestClose(long handle)
    {
        var window = new IntPtr(handle);
        return IsWindow(window) && PostMessage(window, WmClose, IntPtr.Zero, IntPtr.Zero);
    }

    private static string ResolveProcessName(uint processId)
    {
        try
        {
            using var process = Process.GetProcessById(checked((int)processId));
            return process.ProcessName;
        }
        catch (ArgumentException) { return "Unknown process"; }
        catch (InvalidOperationException) { return "Unknown process"; }
        catch (System.ComponentModel.Win32Exception) { return "Protected process"; }
        catch (OverflowException) { return "Unknown process"; }
    }

    private delegate bool EnumWindowsCallback(IntPtr window, IntPtr parameter);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumWindows(EnumWindowsCallback callback, IntPtr parameter);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr window, StringBuilder text, int maxCount);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowTextLength(IntPtr window);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindowVisible(IntPtr window);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindow(IntPtr window);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsIconic(IntPtr window);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr window, out uint processId);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ShowWindow(IntPtr window, int command);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr window);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool PostMessage(IntPtr window, uint message, IntPtr wParam, IntPtr lParam);
}
