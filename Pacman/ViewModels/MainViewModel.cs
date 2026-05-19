using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pacman.Moduls;
using Pacman.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pacman.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly InstalledProgramService _programService = new();
    private readonly UninstallService _uninstallService = new();

    [ObservableProperty]
    private ObservableCollection<InstalledProgram> programs = [];

    public MainViewModel()
    {
        LoadPrograms();
    }

    private void LoadPrograms()
    {
        Programs = new ObservableCollection<InstalledProgram>(
            _programService.GetInstalledPrograms());
    }

    [RelayCommand]
    private void Uninstall(InstalledProgram program)
    {
        _uninstallService.Uninstall(program);
    }
}