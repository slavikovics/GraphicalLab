using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using GraphicalLab.ViewModels;

namespace GraphicalLab.Views;

public partial class TriangulationPage : UserControl
{
    public TriangulationPage()
    {
        InitializeComponent();
        Loaded += SetUp;
    }

    private void SetUp(object? sender, RoutedEventArgs e)
    {
        (DataContext as TriangulationPageViewModel)?.TargetImage = Image;
    }
}