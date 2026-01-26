using System;
using Avalonia.Controls.Notifications;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Toasts;

namespace GraphicalEditor.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public ISukiToastManager ToastManager { get; } = new SukiToastManager();

    [RelayCommand]
    private void Error()
    {
        var builder = ToastManager.CreateToast();
        builder.SetContent("An error occured");
        builder.SetTitle("Error");
        builder.SetType(NotificationType.Error);
        builder.SetCanDismissByClicking(true);
        builder.SetDismissAfter(TimeSpan.FromSeconds(2));
        builder.Queue();
    }
    
    [RelayCommand]
    private void Warning()
    {
        var builder = ToastManager.CreateToast();
        builder.SetContent("A warning occured");
        builder.SetTitle("Warning");
        builder.SetType(NotificationType.Warning);
        builder.SetCanDismissByClicking(true);
        builder.SetDismissAfter(TimeSpan.FromSeconds(2));
        builder.Queue();
    }
    
    [RelayCommand]
    private void Info()
    {
        var builder = ToastManager.CreateToast();
        builder.SetContent("An info occured");
        builder.SetTitle("Info");
        builder.SetType(NotificationType.Information);
        builder.SetCanDismissByClicking(true);
        builder.SetDismissAfter(TimeSpan.FromSeconds(2));
        builder.Queue();
    }
    
    [RelayCommand]
    private void Success()
    {
        var builder = ToastManager.CreateToast();
        builder.SetContent("A success occured");
        builder.SetTitle("Success");
        builder.SetType(NotificationType.Success);
        builder.SetCanDismissByClicking(true);
        builder.SetDismissAfter(TimeSpan.FromSeconds(2));
        builder.Queue();
    }   
}