using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using Windows.Foundation;
using Windows.Graphics;
using WinRT.Interop;

namespace Pacman.Services;

public sealed class PacmanAnimationService
{
    public void PlayPacmanToIcon(Point target)
    {
        var window = new PacmanAnimationWindow(target);
        window.Activate();
    }
}

internal sealed class PacmanAnimationWindow : Window
{
    private readonly Image _pacman;
    private readonly DispatcherQueueTimer _timer;
    private readonly Point _target;

    private double _left = -120;

    public PacmanAnimationWindow(Point target)
    {
        _target = target;

        var root = new Grid
        {
            Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent)
        };

        _pacman = new Image
        {
            Width = 96,
            Height = 96,
            Stretch = Stretch.Fill,
            Source = new BitmapImage(new Uri("ms-appx:///Assets/pacman.gif")),
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(_left, Math.Max(0, target.Y - 32), 0, 0)
        };

        root.Children.Add(_pacman);
        Content = root;

        ExtendsContentIntoTitleBar = true;

        var hwnd = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);

        appWindow.MoveAndResize(new RectInt32(0, 0, 1920, 1080));

        if (appWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.SetBorderAndTitleBar(false, false);
            presenter.IsAlwaysOnTop = true;
            presenter.IsResizable = false;
            presenter.IsMaximizable = false;
            presenter.IsMinimizable = false;
        }

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
        _left += 18;

        _pacman.Margin = new Thickness(
            _left,
            Math.Max(0, _target.Y - 32),
            0,
            0
        );

        if (_left >= _target.X - 40)
        {
            _timer.Stop();
            Close();
        }
    }
}