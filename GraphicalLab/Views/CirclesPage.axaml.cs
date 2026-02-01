using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using GraphicalLab.ViewModels;

namespace GraphicalLab.Views;

public partial class CirclesPage : UserControl
{
    public CirclesPage()
    {
        InitializeComponent();
        Loaded += SetUp;
    }

    private void SetUp(object? sender, RoutedEventArgs e)
    {
        (DataContext as CirclesPageViewModel)?.TargetImage = Image;
    }

    private void Image_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        (DataContext as CirclesPageViewModel)?.HandleClickCommand.Execute(e);
    }
}