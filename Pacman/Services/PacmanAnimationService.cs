using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using Windows.Graphics;
using WinRT.Interop;

namespace Pacman.Services;

public sealed class PacmanAnimationService
{
    public void PlayPacmanEatingAppIcon()
    {
        var window = new PacmanAnimationWindow();
        window.Activate();
    }
}

internal sealed class PacmanAnimationWindow : Window
{
    private readonly Grid _root;
    private readonly Image _pacman;
    private readonly Image _icon;
    private readonly DispatcherQueueTimer _timer;

    private double _pacmanLeft;
    private double _iconLeft;
    private double _centerY;
    private int _tick;

    public PacmanAnimationWindow()
    {
        _root = new Grid
        {
            Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent)
        };

        _icon = new Image
        {
            Width = 96,
            Height = 96,
            Stretch = Stretch.Uniform,
            Source = new BitmapImage(
                new Uri("ms-appx:///Assets/Square150x150Logo.scale-200.png")
            ),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top
        };

        _pacman = new Image
        {
            Width = 120,
            Height = 120,
            Stretch = Stretch.Uniform,
            Source = new BitmapImage(
                new Uri("ms-appx:///Assets/pacman.gif")
            ),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top
        };

        _root.Children.Add(_icon);
        _root.Children.Add(_pacman);

        Content = _root;

        ExtendsContentIntoTitleBar = true;

        var hwnd = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);

        var width = 1920;
        var height = 1080;

        appWindow.MoveAndResize(new RectInt32(0, 0, width, height));

        if (appWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.SetBorderAndTitleBar(false, false);
            presenter.IsAlwaysOnTop = true;
            presenter.IsResizable = false;
            presenter.IsMaximizable = false;
            presenter.IsMinimizable = false;
        }

        _iconLeft = (width / 2.0) - 48;
        _centerY = (height / 2.0) - 48;
        _pacmanLeft = _iconLeft - 420;

        _icon.Margin = new Thickness(_iconLeft, _centerY, 0, 0);
        _pacman.Margin = new Thickness(_pacmanLeft, _centerY - 12, 0, 0);

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
        _tick++;
        _pacmanLeft += 10;

        _pacman.Margin = new Thickness(_pacmanLeft, _centerY - 12, 0, 0);

        if (_pacmanLeft >= _iconLeft - 35)
        {
            _icon.Opacity = Math.Max(0, _icon.Opacity - 0.08);
            _icon.RenderTransform = new ScaleTransform
            {
                ScaleX = Math.Max(0, 1 - (_tick * 0.015)),
                ScaleY = Math.Max(0, 1 - (_tick * 0.015))
            };
        }

        if (_pacmanLeft >= _iconLeft + 120)
        {
            _timer.Stop();
            Close();
        }
    }
}