using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Pacman.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Graphics;
using WinRT.Interop;

namespace Pacman.Services;

public sealed class PacmanAnimationService
{
    private static readonly List<Window> OpenWindows = [];

    public void PlayPacmanEatingAppIcon(InstalledProgram program)
    {
        var window = new PacmanAnimationWindow(program);

        OpenWindows.Add(window);
        window.Closed += (_, _) => OpenWindows.Remove(window);

        window.Activate();
    }
}

internal sealed class PacmanAnimationWindow : Window
{
    private readonly Grid _root;
    private readonly Image _pacman;
    private readonly Image _appIcon;
    private readonly DispatcherQueueTimer _timer;

    private double _pacmanLeft;
    private double _iconLeft;
    private double _iconTop;

    public PacmanAnimationWindow(InstalledProgram program)
    {
        _root = new Grid
        {
            Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent)
        };

        _appIcon = new Image
        {
            Width = 96,
            Height = 96,
            Stretch = Stretch.Uniform,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Source = null
        };

        _ = LoadProgramIconAsync(program);

        _pacman = new Image
        {
            Width = 220,
            Height = 220,
            Stretch = Stretch.Uniform,
            Source = new BitmapImage(new Uri("ms-appx:///Assets/pacman.gif")),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top
        };

        _root.Children.Add(_appIcon);
        _root.Children.Add(_pacman);

        Content = _root;
        ExtendsContentIntoTitleBar = true;

        var hwnd = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);

        var displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary);
        var workArea = displayArea.WorkArea;

        appWindow.MoveAndResize(workArea);

        if (appWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.SetBorderAndTitleBar(false, false);
            presenter.IsAlwaysOnTop = true;
            presenter.IsResizable = false;
            presenter.IsMaximizable = false;
            presenter.IsMinimizable = false;
        }

        _iconLeft = (workArea.Width / 2.0) - 48;
        _iconTop = (workArea.Height / 2.0) - 48;
        _pacmanLeft = _iconLeft - 430;

        _appIcon.Margin = new Thickness(_iconLeft, _iconTop, 0, 0);
        _pacman.Margin = new Thickness(
            _pacmanLeft,
            _iconTop - 62,
            0,
            0
        );

        _timer = DispatcherQueue.CreateTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(16);
        _timer.Tick += OnTick;

        Activated += (_, _) =>
        {
            if (!_timer.IsRunning)
                _timer.Start();
        };
    }

    private void OnTick(DispatcherQueueTimer sender, object args)
    {
        _pacmanLeft += 12;

        _pacman.Margin = new Thickness(
            _pacmanLeft,
            _iconTop - 62,
            0,
            0
        );

        if (_pacmanLeft >= _iconLeft - 30)
        {
            _appIcon.Opacity = Math.Max(0, _appIcon.Opacity - 0.08);
        }

        if (_pacmanLeft >= _iconLeft + 140)
        {
            _timer.Stop();
            Close();
        }
    }

    private static ImageSource TryCreateIconSource(InstalledProgram program)
    {
        var exePath = TryResolveIconPath(program);

        if (!string.IsNullOrWhiteSpace(exePath) && File.Exists(exePath))
        {
            try
            {
                return new BitmapImage(new Uri(exePath));
            }
            catch
            {
            }
        }

        return CreateFallbackIcon();
    }

    private static string? TryResolveIconPath(InstalledProgram program)
    {
        var displayIcon = program.DisplayIcon;

        if (!string.IsNullOrWhiteSpace(displayIcon))
        {
            var path = CleanIconPath(displayIcon);

            if (File.Exists(path))
                return path;
        }

        if (!string.IsNullOrWhiteSpace(program.InstallLocation))
        {
            var exe = Directory
                .EnumerateFiles(program.InstallLocation, "*.exe", SearchOption.TopDirectoryOnly)
                .FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(exe))
                return exe;
        }

        if (!string.IsNullOrWhiteSpace(program.UninstallString))
        {
            var path = CleanIconPath(program.UninstallString);

            if (File.Exists(path))
                return path;
        }

        return null;
    }

    private static string CleanIconPath(string value)
    {
        value = value.Trim();

        if (value.StartsWith("\""))
        {
            var end = value.IndexOf('"', 1);

            if (end > 1)
                return value.Substring(1, end - 1);
        }

        var comma = value.IndexOf(',');

        if (comma > 0)
            value = value[..comma];

        return value.Trim();
    }

    private async Task LoadProgramIconAsync(InstalledProgram program)
    {
        var iconPath = TryResolveIconPath(program);

        System.Diagnostics.Debug.WriteLine($"ICON PATH: {iconPath}");

        if (string.IsNullOrWhiteSpace(iconPath) || !File.Exists(iconPath))
            return;

        var ext = Path.GetExtension(iconPath).ToLowerInvariant();

        if (ext is ".ico" or ".png" or ".jpg" or ".jpeg" or ".bmp")
        {
            _appIcon.Source = new BitmapImage(new Uri(iconPath));
            return;
        }

        if (ext is ".exe" or ".dll")
        {
            var iconSource = await ProgramIconService.ExtractIconAsync(iconPath, 96);

            System.Diagnostics.Debug.WriteLine($"ICON SOURCE NULL: {iconSource == null}");

            if (iconSource != null)
                _appIcon.Source = iconSource;

            return;
        }

        System.Diagnostics.Debug.WriteLine($"Unsupported icon file type: {ext}");
    }

    private static ImageSource CreateFallbackIcon()
    {
        return new BitmapImage(new Uri("ms-appx:///Assets/pacman.gif"));
    }
}