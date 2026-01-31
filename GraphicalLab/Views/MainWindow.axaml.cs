using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using GraphicalLab.ViewModels;
using SukiUI.Controls;

namespace GraphicalLab.Views;

public partial class MainWindow : SukiWindow
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += SetUp;
    }

    private void SetUp(object? sender, RoutedEventArgs e)
    {
        (DataContext as MainWindowViewModel)?.TargetImage = Image;
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        (DataContext as MainWindowViewModel)?.ClearBitmap(Image);
    }

    private void Image_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        (DataContext as MainWindowViewModel)?.HandleClickCommand.Execute(e);
    }
}