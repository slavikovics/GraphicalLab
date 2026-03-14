using Avalonia.Controls;
using Avalonia.Input;
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

    private void Image_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        (DataContext as PolysPageViewModel)?.HandleMoveCommand.Execute(e);
    }

    private void Image_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        (DataContext as PolysPageViewModel)?.HandleClickCommand.Execute(e);
    }
}