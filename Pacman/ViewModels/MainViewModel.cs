using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pacman.Models;
using Pacman.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Pacman.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly InstalledProgramService _programService = new();
    private readonly UninstallService _uninstallService = new();

    private List<InstalledProgram> _allPrograms = [];

    [ObservableProperty]
    private ObservableCollection<InstalledProgram> programs = [];

    [ObservableProperty]
    private string searchText = "";

    public MainViewModel()
    {
        LoadPrograms();
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplyFilter();
    }

    private void LoadPrograms()
    {
        _allPrograms = _programService.GetInstalledPrograms();
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _allPrograms
            : _allPrograms
                .Where(x =>
                    x.DisplayName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    (x.Publisher?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();

        Programs = new ObservableCollection<InstalledProgram>(filtered);
    }

    [RelayCommand]
    private async Task Uninstall(InstalledProgram program)
    {
        var animationService = new PacmanAnimationService();

        animationService.PlayPacmanEatingAppIcon();
        await Task.Delay(1800);

        _uninstallService.Uninstall(program);
    }
}