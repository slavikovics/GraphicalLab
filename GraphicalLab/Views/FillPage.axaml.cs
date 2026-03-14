using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using GraphicalLab.ViewModels;

namespace GraphicalLab.Views;

public partial class FillPage : UserControl
{
    public FillPage()
    {
        InitializeComponent();
        Loaded += SetUp;
    }

    private void SetUp(object? sender, RoutedEventArgs e)
    {
        (DataContext as FillPageViewModel)?.TargetImage = Image;
    }

    private void Image_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        (DataContext as FillPageViewModel)?.HandleClickCommand.Execute(e);
    }
}