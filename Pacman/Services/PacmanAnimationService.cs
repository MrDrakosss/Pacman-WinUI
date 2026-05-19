using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Pacman.Services;

public sealed class PacmanAnimationService
{
    public void PlayPacmanToIcon(Point target)
    {
        var gifPath = Path.Combine(AppContext.BaseDirectory, "Assets", "pacman.gif");

        if (!File.Exists(gifPath))
            return;

        var form = new Form
        {
            FormBorderStyle = FormBorderStyle.None,
            StartPosition = FormStartPosition.Manual,
            TopMost = true,
            ShowInTaskbar = false,
            BackColor = Color.Magenta,
            TransparencyKey = Color.Magenta,
            Bounds = Screen.PrimaryScreen?.Bounds ?? new Rectangle(0, 0, 1920, 1080)
        };

        var picture = new PictureBox
        {
            Image = Image.FromFile(gifPath),
            SizeMode = PictureBoxSizeMode.StretchImage,
            Width = 96,
            Height = 96,
            BackColor = Color.Transparent,
            Left = -120,
            Top = Math.Max(0, target.Y - 32)
        };

        form.Controls.Add(picture);

        var timer = new System.Windows.Forms.Timer
        {
            Interval = 16
        };

        timer.Tick += (_, _) =>
        {
            picture.Left += 18;

            if (picture.Left >= target.X - 40)
            {
                timer.Stop();
                form.Close();
                form.Dispose();
            }
        };

        form.Shown += (_, _) => timer.Start();

        form.Show();
    }
}