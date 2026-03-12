using Avalonia.Controls;
using Avalonia.Interactivity;
using GraphicalLab.ViewModels;

namespace GraphicalLab.Views;

public partial class PolysPage : UserControl
{
    public PolysPage()
    {
        InitializeComponent();
        Loaded += SetUp;
    }

    private void SetUp(object? sender, RoutedEventArgs e)
    {
        (DataContext as PolysPageViewModel)?.TargetImage = Image;
    }
}