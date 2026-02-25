using Avalonia.Controls;
using Avalonia.Interactivity;
using GraphicalLab.ViewModels;

namespace GraphicalLab.Views;

public partial class TransformPage : UserControl
{
    public TransformPage()
    {
        InitializeComponent();
        Loaded += SetUp;
    }

    private void SetUp(object? sender, RoutedEventArgs e)
    {
        (DataContext as TransformPageViewModel)?.TargetImage = Image;
    }
}