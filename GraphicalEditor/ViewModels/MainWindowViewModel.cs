using System;
using Avalonia;
using Avalonia.Controls.Notifications;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Toasts;

namespace GraphicalEditor.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public ISukiToastManager ToastManager { get; } = new SukiToastManager();

    [ObservableProperty] private WriteableBitmap _bitmap = new(new PixelSize(300, 300), new Vector(300, 300));

    private void ShowToast(string title, string content, NotificationType notificationType)
    {
        var builder = ToastManager.CreateToast();
        builder.SetContent(content);
        builder.SetTitle(title);
        builder.SetType(notificationType);
        builder.SetCanDismissByClicking(true);
        builder.SetDismissAfter(TimeSpan.FromSeconds(2));
        builder.Queue();
    }

    [RelayCommand]
    private void Error()
    {
        ShowToast("Error", "An error occured", NotificationType.Error);
    }
    
    [RelayCommand]
    private void Warning()
    {
        ShowToast("Warning", "A warning occured", NotificationType.Warning);
    }
    
    [RelayCommand]
    private void Info()
    {
        ShowToast("Info", "An info occured", NotificationType.Information);
    }
    
    [RelayCommand]
    private void Success()
    {
        ShowToast("Success", "A success occured", NotificationType.Success);
    }   
}