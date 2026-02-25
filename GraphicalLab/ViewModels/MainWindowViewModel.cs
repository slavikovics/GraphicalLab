using GraphicalLab.Services.ToastManagerService;
using SukiUI.Toasts;

namespace GraphicalLab.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IToastManager _toastManager;
    public ISukiToastManager ToastManager => _toastManager.GetToastManager();
    public LinesPageViewModel LinesPageViewModel { get; }
    public CirclesPageViewModel CirclesPageViewModel { get; }
    public CurvesPageViewModel CurvesPageViewModel { get; }
    public TransformPageViewModel TransformPageViewModel { get; }

    public MainWindowViewModel(
        IToastManager toastManager,
        LinesPageViewModel linesPageViewModel,
        CirclesPageViewModel circlesPageViewModel,
        CurvesPageViewModel curvesPageViewModel,
        TransformPageViewModel transformPageViewModel)
    {
        _toastManager = toastManager;
        LinesPageViewModel = linesPageViewModel;
        CirclesPageViewModel = circlesPageViewModel;
        CurvesPageViewModel = curvesPageViewModel;
        TransformPageViewModel = transformPageViewModel;
    }
}