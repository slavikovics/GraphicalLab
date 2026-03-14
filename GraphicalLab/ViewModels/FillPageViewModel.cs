using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GraphicalLab.Fill;
using GraphicalLab.Models;
using GraphicalLab.Services.DebugControlService;
using GraphicalLab.Services.ToastManagerService;

namespace GraphicalLab.ViewModels;

public partial class FillPageViewModel : ViewModelBase
{
    private readonly IToastManager _toastManager;
    private readonly IDebuggableBitmapControl _debuggableBitmapControl;
    private readonly PolysPageViewModel _polysPageViewModel;

    public int BitmapWidth => _debuggableBitmapControl.GetBitmapWidth();
    public int BitmapHeight => _debuggableBitmapControl.GetBitmapHeight();
    public WriteableBitmap Bitmap => _debuggableBitmapControl.GetBitmap();

    public Image? TargetImage = null;
    private Pixel? _firstPoint;

    [ObservableProperty] private bool _isNextStepAvailable;
    [ObservableProperty] private string _stepsCountText;
    [ObservableProperty] private int _selectedFillIndex;

    public bool IsGridVisible
    {
        get;
        set
        {
            SetProperty(ref field, value);
            _debuggableBitmapControl.IsGridVisible = value;
        }
    }

    public bool IsDebugEnabled
    {
        get;
        set
        {
            SetProperty(ref field, value);
            _debuggableBitmapControl.IsDebugEnabled = value;
        }
    }

    [ObservableProperty] private List<string> _lineTypes =
    [
        "Растровая развёртка с упорядоченным списком рёбер", "Растровая развёртка со списком активных рёбер",
        "Простой алгоритм заполнения с затравкой", "Построчный алгоритм заполнения с затравкой"
    ];

    private delegate void DrawLineDelegate(Pixel start, List<Pixel> polygon, uint[,] pixels, int width, int height,
        uint color = 0xFF0000FF);

    private Dictionary<int, DrawLineDelegate> _fillTypesMatch = null!;

    public FillPageViewModel(IToastManager toastManager, IDebuggableBitmapControl debuggableBitmapControl,
        PolysPageViewModel polysPageViewModel)
    {
        _toastManager = toastManager;
        _debuggableBitmapControl = debuggableBitmapControl;
        _polysPageViewModel = polysPageViewModel;
        _debuggableBitmapControl.WritableBitmapChanged += UpdateImage;
        _debuggableBitmapControl.PropertyChanged += DebuggableBitmapControlOnPropertyChanged;
        InitializeFills();
        InitializeProperties();
    }

    private void DebuggableBitmapControlOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_debuggableBitmapControl.IsNextStepAvailable))
        {
            IsNextStepAvailable = _debuggableBitmapControl.IsNextStepAvailable;
        }
        else if (e.PropertyName == nameof(_debuggableBitmapControl.StepsCountText))
        {
            StepsCountText = _debuggableBitmapControl.StepsCountText;
        }
        else if (e.PropertyName == nameof(_debuggableBitmapControl.IsDebugEnabled))
        {
            IsDebugEnabled = _debuggableBitmapControl.IsDebugEnabled;
        }
        else if (e.PropertyName == nameof(_debuggableBitmapControl.IsGridVisible))
        {
            IsGridVisible = _debuggableBitmapControl.IsGridVisible;
        }
    }

    private void InitializeProperties()
    {
        IsGridVisible = _debuggableBitmapControl.IsGridVisible;
        SelectedFillIndex = 0;
        IsNextStepAvailable = _debuggableBitmapControl.IsNextStepAvailable;
        IsDebugEnabled = _debuggableBitmapControl.IsDebugEnabled;
        StepsCountText = _debuggableBitmapControl.StepsCountText;
    }

    private void InitializeFills()
    {
        _fillTypesMatch = new Dictionary<int, DrawLineDelegate>
        {
            { 0, ScanlineWithSortedEdges },
            { 1, ScanlineWithAet },
            { 2, SimpleFloodFill },
            { 3, ScanlineFloodFill }
        };
    }

    [RelayCommand]
    private void HandleClick(PointerPressedEventArgs e)
    {
        var point = e.GetPosition(TargetImage);
        if (TargetImage is null) return;

        double scale = TargetImage.Bounds.Width / BitmapWidth;
        int x = (int)(point.X / scale);
        int y = (int)(point.Y / scale);

        var start = new Pixel(x, y);
        var matrix = _debuggableBitmapControl.GetPixelMatrix();
        var polygon = _polysPageViewModel.Poly.EdgePointsToPixels();

        _fillTypesMatch[SelectedFillIndex].Invoke(start, polygon, matrix, BitmapWidth, BitmapHeight);
    }

    private void UpdateImage()
    {
        TargetImage?.InvalidateVisual();
    }

    private void ScanlineWithSortedEdges(Pixel start, List<Pixel> polygon, uint[,] pixels, int width, int height,
        uint color = 0xFF0000FF)
    {
        var points = FillAlgorithms.ScanlineWithSortedEdges(polygon, pixels, width, height, color);
        _debuggableBitmapControl.AddPoints(points);
        if (!IsDebugEnabled)
            _toastManager.ShowToast("Произведена заливка",
                "Алгоритм: Растровая развёртка с упорядоченным списком рёбер",
                NotificationType.Success);
    }

    private void ScanlineWithAet(Pixel start, List<Pixel> polygon, uint[,] pixels, int width, int height,
        uint color = 0xFF0000FF)
    {
        var points = FillAlgorithms.ScanlineWithAet(polygon, pixels, width, height, color);
        _debuggableBitmapControl.AddPoints(points);
        if (!IsDebugEnabled)
            _toastManager.ShowToast("Произведена заливка",
                "Алгоритм: Растровая развёртка с упорядоченным списком рёбер, использующая список активных рёбер",
                NotificationType.Success);
    }

    private void SimpleFloodFill(Pixel start, List<Pixel> polygon, uint[,] pixels, int width, int height,
        uint color = 0xFF0000FF)
    {
        var points = FillAlgorithms.SimpleFloodFill(start, pixels, width, height, color);
        _debuggableBitmapControl.AddPoints(points);
        if (!IsDebugEnabled)
            _toastManager.ShowToast("Произведена заливка", "Алгоритм: Простой алгоритм заполнения с затравкой",
                NotificationType.Success);
    }

    private void ScanlineFloodFill(Pixel start, List<Pixel> polygon, uint[,] pixels, int width, int height,
        uint color = 0xFF0000FF)
    {
        var points = FillAlgorithms.ScanlineFloodFill(start, pixels, width, height, color);
        _debuggableBitmapControl.AddPoints(points);
        if (!IsDebugEnabled)
            _toastManager.ShowToast("Произведена заливка", "Алгоритм: Построчный алгоритм заполнения с затравкой",
                NotificationType.Success);
    }

    [RelayCommand]
    private void Redraw()
    {
        _polysPageViewModel.Redraw();
        UpdateImage();
    }

    [RelayCommand]
    public void ClearBitmap()
    {
        _debuggableBitmapControl.ClearBitmap();
    }

    [RelayCommand]
    private void HandleDebugNextStep()
    {
        _debuggableBitmapControl.HandleBulk(60);
    }
}