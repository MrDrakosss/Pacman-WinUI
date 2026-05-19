using Pacman.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Pacman.Services;

public sealed class DesktopShortcutInfo
{
    public required string FilePath { get; init; }
    public required string DisplayName { get; init; }
    public required System.Drawing.Point Position { get; init; }
}

public sealed class DesktopShortcutService
{
    public DesktopShortcutInfo? FindDesktopShortcutForProgram(InstalledProgram program)
    {
        var desktopFiles = GetDesktopShortcutFiles();

        foreach (var file in desktopFiles)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);

            if (!IsProbablySameProgram(fileName, program.DisplayName))
                continue;

            var position = DesktopIconPositionReader.TryGetIconPosition(fileName);

            return new DesktopShortcutInfo
            {
                FilePath = file,
                DisplayName = fileName,
                Position = position ?? new System.Drawing.Point(120, 120)
            };
        }

        return null;
    }

    public void DeleteShortcut(DesktopShortcutInfo shortcut)
    {
        if (File.Exists(shortcut.FilePath))
        {
            File.Delete(shortcut.FilePath);
        }
    }

    private static List<string> GetDesktopShortcutFiles()
    {
        var result = new List<string>();

        var userDesktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        var publicDesktop = Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory);

        if (Directory.Exists(userDesktop))
            result.AddRange(Directory.GetFiles(userDesktop, "*.lnk"));

        if (Directory.Exists(publicDesktop))
            result.AddRange(Directory.GetFiles(publicDesktop, "*.lnk"));

        return result;
    }

    private static bool IsProbablySameProgram(string shortcutName, string programName)
    {
        return shortcutName.Contains(programName, StringComparison.OrdinalIgnoreCase) ||
               programName.Contains(shortcutName, StringComparison.OrdinalIgnoreCase);
    }
}