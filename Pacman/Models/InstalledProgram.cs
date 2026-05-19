using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pacman.Models;

public sealed class InstalledProgram
{
    public string DisplayName { get; init; } = "";
    public string? Publisher { get; init; }
    public string? DisplayVersion { get; init; }
    public string? InstallLocation { get; init; }
    public string? DisplayIcon { get; init; }
    public string? UninstallString { get; init; }
    public string? QuietUninstallString { get; init; }

    public bool CanUninstall =>
        !string.IsNullOrWhiteSpace(UninstallString);
}