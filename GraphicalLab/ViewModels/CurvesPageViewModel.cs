using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GraphicalLab.Circles;
using GraphicalLab.Controls.WaypointControl;
using GraphicalLab.Models;
using GraphicalLab.Services.DebugControlService;
using GraphicalLab.Services.ToastManagerService;

namespace GraphicalLab.ViewModels;

public partial class CurvesPageViewModel : ViewModelBase
{
    private readonly IToastManager _toastManager;
    private readonly IDebuggableBitmapControl _debuggableBitmapControl;

    public int BitmapWidth => _debuggableBitmapControl.GetBitmapWidth();
    public int BitmapHeight => _debuggableBitmapControl.GetBitmapHeight();
    public WriteableBitmap Bitmap => _debuggableBitmapControl.GetBitmap();

    public Image? TargetImage = null;

    [ObservableProperty] private bool _isNextStepAvailable;
    [ObservableProperty] private string _stepsCountText;
    [ObservableProperty] private int _selectedCircleIndex;

    [ObservableProperty] private bool _isRadiusVisible;
    [ObservableProperty] private int _radius;

    [ObservableProperty] private bool _isAVisible;
    [ObservableProperty] private int _a;

    [ObservableProperty] private bool _isBVisible;
    [ObservableProperty] private int _b;

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

    [ObservableProperty] private List<string> _circleTypes = ["Эрмит", "Безье", "B-Сплайн"];
    [ObservableProperty] private string _parameterName = "R:";

    private delegate void DrawCircleDelegate(Pixel center, uint color);

    private Dictionary<int, DrawCircleDelegate> _curveTypesMatch = null!;

    public ObservableCollection<WaypointModel> Waypoints { get; } = [];

    public CurvesPageViewModel(IToastManager toastManager, IDebuggableBitmapControl debuggableBitmapControl)
    {
        _toastManager = toastManager;
        _debuggableBitmapControl = debuggableBitmapControl;
        _debuggableBitmapControl.WritableBitmapChanged += UpdateImage;
        _debuggableBitmapControl.PropertyChanged += DebuggableBitmapControlOnPropertyChanged;
        PropertyChanged += OnPropertyChanged;
        InitializeCircles();
        InitializeProperties();
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SelectedCircleIndex))
        {
            IsRadiusVisible = CircleTypes[SelectedCircleIndex] == "Окружность" ||
                              CircleTypes[SelectedCircleIndex] == "Парабола";
            IsAVisible = CircleTypes[SelectedCircleIndex] != "Окружность" &&
                         CircleTypes[SelectedCircleIndex] != "Парабола";
            IsBVisible = CircleTypes[SelectedCircleIndex] != "Окружность" &&
                         CircleTypes[SelectedCircleIndex] != "Парабола";
            if (CircleTypes[SelectedCircleIndex] == "Окружность") ParameterName = "R:";
            else ParameterName = "P:";
        }
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
        SelectedCircleIndex = 0;
        IsRadiusVisible = true;
        IsAVisible = false;
        IsBVisible = false;
        Radius = 10;
        A = 2;
        B = 3;
        IsNextStepAvailable = _debuggableBitmapControl.IsNextStepAvailable;
        IsDebugEnabled = _debuggableBitmapControl.IsDebugEnabled;
        StepsCountText = _debuggableBitmapControl.StepsCountText;
    }

    private void InitializeCircles()
    {
        _curveTypesMatch = new Dictionary<int, DrawCircleDelegate>();
        var ddaDelegate = new DrawCircleDelegate(DrawErmit);
        var brezenhemDelegate = new DrawCircleDelegate(DrawBezie);
        var wuDelegate = new DrawCircleDelegate(DrawSpline);

        _curveTypesMatch.Add(0, DrawErmit);
        _curveTypesMatch.Add(1, DrawBezie);
        _curveTypesMatch.Add(2, DrawSpline);
    }

    [RelayCommand]
    private void HandleClick(PointerPressedEventArgs e)
    {
        var point = e.GetPosition(TargetImage);
        if (TargetImage is null) return;

        double scale = TargetImage.Bounds.Width / BitmapWidth;
        int x = (int)(point.X / scale);
        int y = (int)(point.Y / scale);

        var center = new Pixel(x, y);
        _curveTypesMatch[SelectedCircleIndex].Invoke(center, 0xFF0000FF);
    }

    [RelayCommand]
    private void AddWaypoint(Point center)
    {
        Waypoints.Add(new WaypointModel { X = center.X, Y = center.Y });
    }

    [RelayCommand]
    private void WaypointClicked(WaypointModel? model)
    {
        
    }
    
    [RelayCommand]
    private void WaypointDragged(WaypointModel? model)
    {
        
    }

    private void UpdateImage()
    {
        TargetImage?.InvalidateVisual();
    }

    private void DrawErmit(Pixel center, uint color = 0xFF0000FF)
    {
        var points = CircleGenerator.DrawCircle(center, Radius, color);
        _debuggableBitmapControl.AddPoints(points);
        if (!IsDebugEnabled)
            _toastManager.ShowToast("Нарисована окружность", $"Центр: {center}, Радиус: {Radius}",
                NotificationType.Success);
    }

    private void DrawBezie(Pixel center, uint color = 0xFF0000FF)
    {
        var points = EllipseGenerator.DrawEllipse(center, A, B, color);
        _debuggableBitmapControl.AddPoints(points);
        if (!IsDebugEnabled)
            _toastManager.ShowToast("Нарисован эллипс", $"Центр: {center}, A: {A}, B: {B}",
                NotificationType.Success);
    }

    private void DrawSpline(Pixel center, uint color = 0xFF0000FF)
    {
        var points = HyperbolaGenerator.DrawHyperbola(center, A, B, BitmapWidth, BitmapHeight, color);
        _debuggableBitmapControl.AddPoints(points);
        if (!IsDebugEnabled)
            _toastManager.ShowToast("Нарисована гипербола", $"Центр: {center}, A: {A}, B: {B}",
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