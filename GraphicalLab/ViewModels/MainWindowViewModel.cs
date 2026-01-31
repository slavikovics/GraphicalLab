using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GraphicalLab.Lines;
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