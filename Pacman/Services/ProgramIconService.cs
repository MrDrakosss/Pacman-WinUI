using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Security.Cryptography;

namespace Pacman.Services;

public static class ProgramIconService
{
    public static async Task<SoftwareBitmapSource?> ExtractIconAsync(string exePath, int size = 96)
    {
        var hIcon = GetShellIcon(exePath);

        if (hIcon == IntPtr.Zero)
            return null;

        try
        {
            var pixels = IconToBgraPixels(hIcon, size, size);

            if (pixels == null)
                return null;

            var buffer = CryptographicBuffer.CreateFromByteArray(pixels);

            using var bitmap = SoftwareBitmap.CreateCopyFromBuffer(
                buffer,
                BitmapPixelFormat.Bgra8,
                size,
                size,
                BitmapAlphaMode.Premultiplied
            );

            var source = new SoftwareBitmapSource();
            await source.SetBitmapAsync(bitmap);

            return source;
        }
        finally
        {
            DestroyIcon(hIcon);
        }
    }

    private static IntPtr GetShellIcon(string path)
    {
        var info = new SHFILEINFO();

        SHGetFileInfo(
            path,
            0,
            ref info,
            (uint)Marshal.SizeOf<SHFILEINFO>(),
            SHGFI_ICON | SHGFI_LARGEICON
        );

        return info.hIcon;
    }

    private static byte[]? IconToBgraPixels(IntPtr hIcon, int width, int height)
    {
        var hdc = CreateCompatibleDC(IntPtr.Zero);

        if (hdc == IntPtr.Zero)
            return null;

        IntPtr hBitmap = IntPtr.Zero;
        IntPtr oldObject = IntPtr.Zero;

        try
        {
            var bmi = new BITMAPINFO
            {
                bmiHeader = new BITMAPINFOHEADER
                {
                    biSize = (uint)Marshal.SizeOf<BITMAPINFOHEADER>(),
                    biWidth = width,
                    biHeight = -height,
                    biPlanes = 1,
                    biBitCount = 32,
                    biCompression = BI_RGB
                }
            };

            hBitmap = CreateDIBSection(
                hdc,
                ref bmi,
                DIB_RGB_COLORS,
                out var bits,
                IntPtr.Zero,
                0
            );

            if (hBitmap == IntPtr.Zero || bits == IntPtr.Zero)
                return null;

            oldObject = SelectObject(hdc, hBitmap);

            DrawIconEx(
                hdc,
                0,
                0,
                hIcon,
                width,
                height,
                0,
                IntPtr.Zero,
                DI_NORMAL
            );

            var length = width * height * 4;
            var pixels = new byte[length];

            Marshal.Copy(bits, pixels, 0, length);

            return pixels;
        }
        finally
        {
            if (oldObject != IntPtr.Zero)
                SelectObject(hdc, oldObject);

            if (hBitmap != IntPtr.Zero)
                DeleteObject(hBitmap);

            DeleteDC(hdc);
        }
    }

    private const uint SHGFI_ICON = 0x000000100;
    private const uint SHGFI_LARGEICON = 0x000000000;
    private const uint BI_RGB = 0;
    private const uint DIB_RGB_COLORS = 0;
    private const uint DI_NORMAL = 0x0003;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct SHFILEINFO
    {
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BITMAPINFO
    {
        public BITMAPINFOHEADER bmiHeader;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct BITMAPINFOHEADER
    {
        public uint biSize;
        public int biWidth;
        public int biHeight;
        public ushort biPlanes;
        public ushort biBitCount;
        public uint biCompression;
        public uint biSizeImage;
        public int biXPelsPerMeter;
        public int biYPelsPerMeter;
        public uint biClrUsed;
        public uint biClrImportant;
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr SHGetFileInfo(
        string pszPath,
        uint dwFileAttributes,
        ref SHFILEINFO psfi,
        uint cbFileInfo,
        uint uFlags
    );

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DestroyIcon(IntPtr hIcon);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern bool DeleteDC(IntPtr hdc);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern IntPtr CreateDIBSection(
        IntPtr hdc,
        ref BITMAPINFO pbmi,
        uint usage,
        out IntPtr ppvBits,
        IntPtr hSection,
        uint offset
    );

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern bool DeleteObject(IntPtr hObject);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool DrawIconEx(
        IntPtr hdc,
        int xLeft,
        int yTop,
        IntPtr hIcon,
        int cxWidth,
        int cyHeight,
        uint istepIfAniCur,
        IntPtr hbrFlickerFreeDraw,
        uint diFlags
    );
}