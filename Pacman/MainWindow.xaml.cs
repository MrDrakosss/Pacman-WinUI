using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pacman.Moduls;
using Pacman.ViewModels;

namespace Pacman;

public sealed partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; } = new();

    public MainWindow()
    {
        this.InitializeComponent();

        RootGrid.DataContext = ViewModel;
    }

    private void UninstallButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button &&
            button.DataContext is InstalledProgram program)
        {
            ViewModel.UninstallCommand.Execute(program);
        }
    }
}