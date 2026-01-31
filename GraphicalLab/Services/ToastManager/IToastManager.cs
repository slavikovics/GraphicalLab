using Avalonia.Controls.Notifications;
using SukiUI.Toasts;

namespace GraphicalLab;

public interface IToastManager
{
    void ShowToast(string title, string content, NotificationType notificationType);
    ISukiToastManager GetToastManager();
}