using GraphicalLab.Services.ToastManagerService;
using SukiUI.Toasts;

namespace GraphicalLab.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IToastManager _toastManager;
    public ISukiToastManager ToastManager => _toastManager.GetToastManager();
    public LinesPageViewModel LinesPageViewModel { get; }

    public MainWindowViewModel(IToastManager toastManager, LinesPageViewModel linesPageViewModel)
    {
        _toastManager = toastManager;
        LinesPageViewModel = linesPageViewModel;
    }
}