using Avalonia.Controls;
using Avalonia.Interactivity;
using GraphicalLab.ViewModels;

namespace GraphicalLab.Views;

public partial class CurvesPage : UserControl
{
    public CurvesPage()
    {
        InitializeComponent();
        Loaded += SetUp;
    }

    private void SetUp(object? sender, RoutedEventArgs e)
    {
        (DataContext as CurvesPageViewModel)?.TargetImage = Image;
    }
}