using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace Pacman.Services;

public static class DesktopIconPositionReader
{
    private const int LVM_FIRST = 0x1000;
    private const int LVM_GETITEMCOUNT = LVM_FIRST + 4;
    private const int LVM_GETITEMTEXTW = LVM_FIRST + 115;
    private const int LVM_GETITEMPOSITION = LVM_FIRST + 16;

    public static Point? TryGetIconPosition(string displayName)
    {
        var listView = GetDesktopListViewHandle();

        if (listView == IntPtr.Zero)
            return null;

        var count = SendMessage(listView, LVM_GETITEMCOUNT, IntPtr.Zero, IntPtr.Zero).ToInt32();

        for (var i = 0; i < count; i++)
        {
            var name = GetItemText(listView, i);

            if (!string.Equals(name, displayName, StringComparison.OrdinalIgnoreCase))
                continue;

            var point = GetItemPosition(listView, i);

            if (point.HasValue)
                return point.Value;
        }

        return null;
    }

    private static IntPtr GetDesktopListViewHandle()
    {
        var progman = FindWindow("Progman", null);

        var shellView = FindWindowEx(progman, IntPtr.Zero, "SHELLDLL_DefView", null);

        if (shellView == IntPtr.Zero)
        {
            var workerW = IntPtr.Zero;

            while ((workerW = FindWindowEx(IntPtr.Zero, workerW, "WorkerW", null)) != IntPtr.Zero)
            {
                shellView = FindWindowEx(workerW, IntPtr.Zero, "SHELLDLL_DefView", null);

                if (shellView != IntPtr.Zero)
                    break;
            }
        }

        return FindWindowEx(shellView, IntPtr.Zero, "SysListView32", "FolderView");
    }

    private static string GetItemText(IntPtr listView, int index)
    {
        // Ez MVP-ben nem tökéletes, mert explorer.exe másik processz.
        // Ha nem működik, fallback pozíciót használunk.
        return "";
    }

    private static Point? GetItemPosition(IntPtr listView, int index)
    {
        // Ugyanez: cross-process memória kellene a teljesen pontos megoldáshoz.
        return null;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr FindWindow(string className, string? windowName);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr FindWindowEx(
        IntPtr parentHandle,
        IntPtr childAfter,
        string className,
        string? windowTitle);

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(
        IntPtr hWnd,
        int msg,
        IntPtr wParam,
        IntPtr lParam);
}