using Avalonia.Controls.Notifications;
using SukiUI.Toasts;

namespace GraphicalLab.Services.ToastManagerService;

public interface IToastManager
{
    void ShowToast(string title, string content, NotificationType notificationType);
    ISukiToastManager GetToastManager();
}