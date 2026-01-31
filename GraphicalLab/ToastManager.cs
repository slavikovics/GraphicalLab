using System;
using Avalonia.Controls.Notifications;
using SukiUI.Toasts;

namespace GraphicalLab;

public class ToastManager : IToastManager
{
    private ISukiToastManager Toasts { get; }

    public ToastManager()
    {
        Toasts = new SukiToastManager();
    }
    
    public void ShowToast(string title, string content, NotificationType notificationType)
    {
        var builder = Toasts.CreateToast();
        builder.SetContent(content);
        builder.SetTitle(title);
        builder.SetType(notificationType);
        builder.SetCanDismissByClicking(true);
        builder.SetDismissAfter(TimeSpan.FromSeconds(2));
        builder.Queue();
    }

    public ISukiToastManager GetToastManager()
    {
        return Toasts;
    }
}