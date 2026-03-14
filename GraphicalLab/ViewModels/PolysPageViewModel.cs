using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Remote.Protocol.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GraphicalLab.Controls.WaypointControl;
using GraphicalLab.Lines;
using GraphicalLab.Models;
using GraphicalLab.Poly;
using GraphicalLab.Services.DebugControlService;
using GraphicalLab.Services.ToastManagerService;

namespace GraphicalLab.ViewModels;

public partial class PolysPageViewModel : ViewModelBase
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
    [ObservableProperty] private int _selectedPolyIndex;

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
            Redraw();
        }
    }

    [ObservableProperty]
    private List<string> _polyTypes = ["Построение", "Грэхем", "Джарвис", "Пересечение с отрезком"];

    private delegate void DrawPolyDelegate();

    private Dictionary<int, DrawPolyDelegate> _polyTypesMatch = null!;
    public ObservableCollection<WaypointModel> Waypoints { get; } = [];
    private Poly.Poly _poly;

    public PolysPageViewModel(IToastManager toastManager, IDebuggableBitmapControl debuggableBitmapControl)
    {
        _toastManager = toastManager;
        _debuggableBitmapControl = debuggableBitmapControl;
        _debuggableBitmapControl.WritableBitmapChanged += UpdateImage;
        _debuggableBitmapControl.PropertyChanged += DebuggableBitmapControlOnPropertyChanged;
        PropertyChanged += OnPropertyChanged;
        InitializePolys();
        InitializeProperties();
    }

    [ObservableProperty] private bool _normsEnabled;
    [ObservableProperty] private bool _autoNorms;
    [ObservableProperty] private bool _buildEnabled;
    [ObservableProperty] private bool _hullBuildEnabled;
    [ObservableProperty] private bool _addLineEnabled;
    [ObservableProperty] private ConvexResult _convexResult;
    [ObservableProperty] private PointInfo _pointInfo;

    private void SetAllToFalse()
    {
        NormsEnabled = false;
        AutoNorms = false;
        BuildEnabled = false;
        HullBuildEnabled = false;
        AddLineEnabled = false;
        _firstPoint = null;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SelectedPolyIndex))
        {
            switch (SelectedPolyIndex)
            {
                case 0:
                    SetAllToFalse();
                    NormsEnabled = true;
                    BuildEnabled = true;
                    ClearBitmap();
                    break;
                case 1:
                    SetAllToFalse();
                    HullBuildEnabled = true;
                    BuildHull();
                    break;
                case 2:
                    SetAllToFalse();
                    HullBuildEnabled = true;
                    BuildHull();
                    break;
                case 3:
                    SetAllToFalse();
                    AddLineEnabled = true;
                    break;
            }

            Redraw();
        }

        if (e.PropertyName == nameof(AutoNorms)) Redraw();
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
        _poly = new Poly.Poly(new Size(_debuggableBitmapControl.GetBitmapWidth(),
            _debuggableBitmapControl.GetBitmapHeight()));
        SelectedPolyIndex = 0;
        NormsEnabled = true;
        BuildEnabled = true;
        PointInfo = new PointInfo();
        IsNextStepAvailable = _debuggableBitmapControl.IsNextStepAvailable;
        IsDebugEnabled = _debuggableBitmapControl.IsDebugEnabled;
        StepsCountText = _debuggableBitmapControl.StepsCountText;
    }

    private void InitializePolys()
    {
        _polyTypesMatch = new Dictionary<int, DrawPolyDelegate>
        {
            { 1, DrawGraham },
            { 2, DrawJarvis }
        };
    }

    private void DrawGraham()
    {
        var canvasSize = new Size(_debuggableBitmapControl.GetBitmapWidth(),
            _debuggableBitmapControl.GetBitmapHeight());
        var points = WaypointModel.ToPixels(Waypoints.ToList(), canvasSize);
        var newPoints = Graham.Draw(points);

        _poly.Clear();
        _poly.AddRange(newPoints);
        _poly.Close(null);
        Redraw();
    }

    private void DrawJarvis()
    {
        var canvasSize = new Size(_debuggableBitmapControl.GetBitmapWidth(),
            _debuggableBitmapControl.GetBitmapHeight());
        var points = WaypointModel.ToPixels(Waypoints.ToList(), canvasSize);
        var newPoints = Jarvis.Draw(points);

        _poly.Clear();
        _poly.AddRange(newPoints);
        _poly.Close(null);
        Redraw();
    }

    [RelayCommand]
    private void BuildHull()
    {
        _polyTypesMatch[SelectedPolyIndex].Invoke();
    }

    [RelayCommand]
    private void AddWaypoint(Point center)
    {
        var newWaypoint = new WaypointModel { X = center.X, Y = center.Y };
        Waypoints.Add(newWaypoint);
        
        if (BuildEnabled) _poly.AddPoint(newWaypoint);
        else if (HullBuildEnabled) BuildHull();
        Redraw();
    }

    [RelayCommand]
    private void WaypointClicked(WaypointModel? model)
    {
        if (!BuildEnabled) return;
        _poly.Close(model);
        Redraw();
    }

    [RelayCommand]
    private void DrawNorms()
    {
        var pixels = _poly.DrawNorms();
        _debuggableBitmapControl.AddPoints(pixels);
    }

    [RelayCommand]
    private void WaypointDragged(WaypointModel? model)
    {
        if (!BuildEnabled && HullBuildEnabled) BuildHull();
        else Redraw();
    }

    [RelayCommand]
    private void HandleClick(PointerPressedEventArgs e)
    {
        if (!AddLineEnabled) return;
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
            var pixels = BrezenhemLineGenerator.DrawLine(_firstPoint, new Pixel(x, y), 0xFF228B22);
            Redraw();
            _debuggableBitmapControl.AddPoints(pixels);
            if (!IsDebugEnabled)
                _toastManager.ShowToast("Нарисован отрезок", $"Пересечения с полигоном:",
                    NotificationType.Success);
            _firstPoint = null;
        }
    }

    [RelayCommand]
    private void HandleMove(PointerEventArgs e)
    {
        var position = e.GetPosition(TargetImage);
        if (TargetImage is null) return;

        double scale = TargetImage.Bounds.Width / BitmapWidth;
        int x = (int)(position.X / scale);
        int y = (int)(position.Y / scale);

        var point = new Pixel(x, y);
        PointInfo.Point = point;
        PointInfo.IsInside = InsideChecker.IsInside(point, _poly.EdgePointsToPixels());
    }

    private void UpdateImage()
    {
        TargetImage?.InvalidateVisual();
    }

    private void Redraw()
    {
        List<Pixel> pixels = [];
        pixels.AddRange(_poly.Draw());
        if (AutoNorms) pixels.AddRange(_poly.DrawNorms());
        ConvexResult = _poly.Convex();

        _debuggableBitmapControl.ClearBitmap(true);
        _debuggableBitmapControl.AddPoints(pixels);
    }

    [RelayCommand]
    public void ClearBitmap()
    {
        Waypoints.Clear();
        _poly.Clear();
        _debuggableBitmapControl.ClearBitmap();
    }

    [RelayCommand]
    private void HandleDebugNextStep()
    {
        _debuggableBitmapControl.HandleDebugNextStep();
    }
}