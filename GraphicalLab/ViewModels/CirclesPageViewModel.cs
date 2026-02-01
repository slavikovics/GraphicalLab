using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GraphicalLab.Lines;
using GraphicalLab.Services.DebugControlService;
using GraphicalLab.Services.ToastManagerService;
using GraphicalLab.Services.WritableBitmapProviderService;

namespace GraphicalLab.ViewModels;

public partial class CirclesPageViewModel : ViewModelBase
{
    private readonly IToastManager _toastManager;
    private readonly IDebuggableBitmapControl _debuggableBitmapControl;

    public int BitmapWidth => _debuggableBitmapControl.GetBitmapWidth();
    public int BitmapHeight => _debuggableBitmapControl.GetBitmapHeight();
    public WriteableBitmap Bitmap => _debuggableBitmapControl.GetBitmap();
    
    public Image? TargetImage = null;
    private Pixel? _firstPoint;

    [ObservableProperty] private bool _isNextStepAvailable;
    [ObservableProperty] private string _stepsCountText;
    [ObservableProperty] private int _selectedLineIndex;
    
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

    [ObservableProperty] private List<string> _lineTypes = ["ЦДА", "Брезенхем", "Ву"];

    private delegate void DrawLineDelegate(Pixel start, Pixel end, uint color);

    private Dictionary<int, DrawLineDelegate> _lineTypesMatch = null!;

    public CirclesPageViewModel(IToastManager toastManager, IDebuggableBitmapControl debuggableBitmapControl)
    {
        _toastManager = toastManager;
        _debuggableBitmapControl = debuggableBitmapControl;
        _debuggableBitmapControl.WritableBitmapChanged += UpdateImage;
        _debuggableBitmapControl.PropertyChanged += DebuggableBitmapControlOnPropertyChanged;
        InitializeLines();
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
        SelectedLineIndex = 0;
        IsNextStepAvailable =_debuggableBitmapControl.IsNextStepAvailable;
        IsDebugEnabled = _debuggableBitmapControl.IsDebugEnabled;
        StepsCountText = _debuggableBitmapControl.StepsCountText;
    }

    private void InitializeLines()
    {
        _lineTypesMatch = new Dictionary<int, DrawLineDelegate>();
        var ddaDelegate = new DrawLineDelegate(DrawLineDda);
        var brezenhemDelegate = new DrawLineDelegate(DrawLineBrezenhem);
        var wuDelegate = new DrawLineDelegate(DrawLineWu);

        _lineTypesMatch.Add(0, DrawLineDda);
        _lineTypesMatch.Add(1, DrawLineBrezenhem);
        _lineTypesMatch.Add(2, DrawLineWu);
    }

    [RelayCommand]
    private void HandleClick(PointerPressedEventArgs e)
    {
        var point = e.GetPosition(TargetImage);
        if (TargetImage is null) return;

        double scale = TargetImage.Bounds.Width / BitmapWidth;
        int x = (int)(point.X / scale);
        int y = (int)(point.Y / scale);

        _debuggableBitmapControl.SetPixel(new Pixel(x, y));

        if (_firstPoint is null)
        {
            _firstPoint = new Pixel(x, y);
        }
        else
        {
            _lineTypesMatch[SelectedLineIndex].Invoke(_firstPoint, new Pixel(x, y), 0xFF0000FF);
            _firstPoint = null;
        }
    }

    private void UpdateImage()
    {
        TargetImage?.InvalidateVisual();
    }

    private void DrawLineDda(Pixel start, Pixel end, uint color = 0xFF0000FF)
    {
        var points = DdaLineGenerator.DrawLine(start, end, color);
        _debuggableBitmapControl.AddPoints(points);
        if (!IsDebugEnabled) _toastManager.ShowToast("Нарисован отрезок", $"Алгоритм: ЦДА, Начало: {start}, Конец: {end}",
            NotificationType.Success);
    }

    private void DrawLineBrezenhem(Pixel start, Pixel end, uint color = 0xFF0000FF)
    {
        var points = BrezenhemLineGenerator.DrawLine(start, end, color);
        _debuggableBitmapControl.AddPoints(points);
        if (!IsDebugEnabled) _toastManager.ShowToast("Нарисован отрезок", $"Алгоритм: Брезенхем, Начало: {start}, Конец: {end}",
            NotificationType.Success);
    }

    private void DrawLineWu(Pixel start, Pixel end, uint color = 0xFF0000FF)
    {
        var points = XiaolinWuLineGenerator.DrawLine(start, end, color);
        _debuggableBitmapControl.AddPoints(points);
        if (!IsDebugEnabled) _toastManager.ShowToast("Нарисован отрезок", $"Алгоритм: Ву, Начало: {start}, Конец: {end}",
            NotificationType.Success);
    }

    [RelayCommand]
    public void ClearBitmap()
    {
        _debuggableBitmapControl.ClearBitmap();
    }

    [RelayCommand]
    private void HandleDebugNextStep()
    {
        _debuggableBitmapControl.HandleDebugNextStep();
    }
}