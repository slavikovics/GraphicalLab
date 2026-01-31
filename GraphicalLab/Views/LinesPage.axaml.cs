using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using GraphicalLab.ViewModels;

namespace GraphicalLab.Views;

public partial class LinesPage : UserControl
{
    public LinesPage()
    {
        InitializeComponent();
        Loaded += SetUp;
    }

    private void SetUp(object? sender, RoutedEventArgs e)
    {
        (DataContext as LinesPageViewModel)?.TargetImage = Image;
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        (DataContext as LinesPageViewModel)?.ClearBitmap(Image);
    }

    private void Image_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        (DataContext as LinesPageViewModel)?.HandleClickCommand.Execute(e);
    }
}