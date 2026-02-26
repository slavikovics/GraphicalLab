using Avalonia.Input;
using GraphicalLab.ViewModels;
using SukiUI.Controls;

namespace GraphicalLab.Views;

public partial class MainWindow : SukiWindow
{
    public MainWindow()
    {
        InitializeComponent();
        KeyDown += OnKeyDown;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        (DataContext as MainWindowViewModel)?.HandleKeyDownCommandCommand.Execute(e);
    }
}