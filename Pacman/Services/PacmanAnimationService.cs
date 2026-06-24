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
            Source = CreateFallbackIcon()
        };

        _ = LoadProgramIconAsync(program);

        _pacman = new Image
        {
            Width = 120,
            Height = 120,
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
        _pacman.Margin = new Thickness(_pacmanLeft, _iconTop - 12, 0, 0);

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

        _pacman.Margin = new Thickness(_pacmanLeft, _iconTop - 12, 0, 0);

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
        var exePath = TryResolveIconExePath(program);

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

    private static string? TryResolveIconExePath(InstalledProgram program)
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
        var path = value.Trim();

        if (path.StartsWith("\""))
        {
            var endQuote = path.IndexOf('"', 1);

            if (endQuote > 1)
                path = path.Substring(1, endQuote - 1);
        }
        else
        {
            var commaIndex = path.IndexOf(',');

            if (commaIndex > 0)
                path = path.Substring(0, commaIndex);

            var exeIndex = path.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);

            if (exeIndex > 0)
                path = path.Substring(0, exeIndex + 4);
        }

        return path.Trim();
    }

    private async Task LoadProgramIconAsync(InstalledProgram program)
    {
        var exePath = TryResolveIconExePath(program);

        if (string.IsNullOrWhiteSpace(exePath) || !File.Exists(exePath))
            return;

        var iconSource = await ProgramIconService.ExtractIconAsync(exePath, 96);

        if (iconSource != null)
            _appIcon.Source = iconSource;
    }

    private static ImageSource CreateFallbackIcon()
    {
        return new BitmapImage(new Uri("ms-appx:///Assets/pacman.gif"));
    }
}