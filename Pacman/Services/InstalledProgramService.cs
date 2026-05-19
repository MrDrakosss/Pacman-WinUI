using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pacman.Services;

public sealed class InstalledProgramService
{
    private static readonly string[] RegistryPaths =
    {
        @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
        @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
    };

    public List<InstalledProgram> GetInstalledPrograms()
    {
        var result = new List<InstalledProgram>();

        foreach (var hive in new[] { Registry.LocalMachine, Registry.CurrentUser })
        {
            foreach (var path in RegistryPaths)
            {
                using var key = hive.OpenSubKey(path);

                if (key == null)
                    continue;

                foreach (var subKeyName in key.GetSubKeyNames())
                {
                    using var subKey = key.OpenSubKey(subKeyName);

                    if (subKey == null)
                        continue;

                    var name = subKey.GetValue("DisplayName") as string;

                    if (string.IsNullOrWhiteSpace(name))
                        continue;

                    result.Add(new InstalledProgram
                    {
                        DisplayName = name,
                        Publisher = subKey.GetValue("Publisher") as string,
                        DisplayVersion = subKey.GetValue("DisplayVersion") as string,
                        InstallLocation = subKey.GetValue("InstallLocation") as string,
                        DisplayIcon = subKey.GetValue("DisplayIcon") as string,
                        UninstallString = subKey.GetValue("UninstallString") as string,
                        QuietUninstallString = subKey.GetValue("QuietUninstallString") as string
                    });
                }
            }
        }

        return result
            .GroupBy(x => x.DisplayName)
            .Select(x => x.First())
            .OrderBy(x => x.DisplayName)
            .ToList();
    }
}
