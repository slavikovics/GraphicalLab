using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using GraphicalEditor.ViewModels;
using SukiUI.Controls;

namespace GraphicalEditor.Views;

public partial class MainWindow : SukiWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        (DataContext as MainWindowViewModel)?.HandleClick(sender, e.GetPosition((Visual)sender));
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        (DataContext as MainWindowViewModel)?.ClearBitmap(Image);
    }
}