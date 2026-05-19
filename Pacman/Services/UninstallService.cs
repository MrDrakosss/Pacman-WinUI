using Pacman.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pacman.Services;

public sealed class UninstallService
{
    public void Uninstall(InstalledProgram program)
    {
        var command =
            program.QuietUninstallString ??
            program.UninstallString;

        if (string.IsNullOrWhiteSpace(command))
            return;

        Process.Start(new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = "/c " + command,
            UseShellExecute = true,
            Verb = "runas"
        });
    }
}
