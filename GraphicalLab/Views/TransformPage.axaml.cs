using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using GraphicalLab.ViewModels;

namespace GraphicalLab.Views;

public partial class TransformPage : UserControl
{
    public TransformPage()
    {
        InitializeComponent();
        Loaded += SetUp;
        KeyDown += OnKeyDown;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        (DataContext as TransformPageViewModel)?.HandleKeyDownCommand.Execute(e);
    }

    private void SetUp(object? sender, RoutedEventArgs e)
    {
        (DataContext as TransformPageViewModel)?.TargetImage = Image;
    }
}