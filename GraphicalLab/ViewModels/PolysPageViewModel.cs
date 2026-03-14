using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Media.Imaging;
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
        }
    }

    [ObservableProperty]
    private List<string> _polyTypes = ["Построение", "Грэхем", "Джарвис", "Пересечение с отрезком"];

    private delegate void DrawPolyDelegate();

    private Dictionary<int, DrawPolyDelegate> _polyTypesMatch = null!;
    public ObservableCollection<WaypointModel> Waypoints { get; } = [];
    public Poly.Poly Poly;

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
        Poly = new Poly.Poly(new Size(_debuggableBitmapControl.GetBitmapWidth(),
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

        Poly.Clear();
        Poly.AddRange(newPoints);
        Poly.Close(null);
        Redraw();
    }

    private void DrawJarvis()
    {
        var canvasSize = new Size(_debuggableBitmapControl.GetBitmapWidth(),
            _debuggableBitmapControl.GetBitmapHeight());
        var points = WaypointModel.ToPixels(Waypoints.ToList(), canvasSize);
        var newPoints = Jarvis.Draw(points);

        Poly.Clear();
        Poly.AddRange(newPoints);
        Poly.Close(null);
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
        if (AddLineEnabled)
        {
            DrawLine(center);
            return;
        }

        var newWaypoint = new WaypointModel { X = center.X, Y = center.Y };
        Waypoints.Add(newWaypoint);

        if (BuildEnabled) Poly.AddPoint(newWaypoint);
        else if (HullBuildEnabled) BuildHull();
        Redraw();
    }

    [RelayCommand]
    private void WaypointClicked(WaypointModel? model)
    {
        if (!BuildEnabled) return;
        Poly.Close(model);
        Redraw();
    }

    [RelayCommand]
    private void DrawNorms()
    {
        var pixels = Poly.DrawNorms();
        _debuggableBitmapControl.AddPoints(pixels);
    }

    [RelayCommand]
    private void WaypointDragged(WaypointModel? model)
    {
        if (!BuildEnabled && HullBuildEnabled) BuildHull();
        else Redraw();
    }

    private void DrawLine(Point point)
    {
        if (TargetImage is null) return;

        int x = (int)(point.X * _debuggableBitmapControl.GetBitmapWidth());
        int y = (int)(point.Y * _debuggableBitmapControl.GetBitmapHeight());

        _debuggableBitmapControl.SetPixel(new Pixel(x, y, 0xFF228B22));

        if (_firstPoint is null)
        {
            _firstPoint = new Pixel(x, y);
        }
        else
        {
            var secondPoint = new Pixel(x, y);
            var pixels = BrezenhemLineGenerator.DrawLine(_firstPoint, secondPoint, 0xFF228B22);
            pixels.AddRange(Poly.Draw());

            _debuggableBitmapControl.ClearBitmap();
            _debuggableBitmapControl.AddPoints(pixels);

            var intersections =
                IntersectionCalculator.FindAllIntersections(_firstPoint, secondPoint, Poly.EdgePointsToPixels());

            if (!IsDebugEnabled)
            {
                List<Pixel> intersectionPoints = [];
                foreach (var intersection in intersections)
                {
                    _toastManager.ShowToast("Пересечение", intersection.ToString(), NotificationType.Information);
                    intersection.Point.Color = 0xFF8B0000;
                    intersectionPoints.Add(intersection.Point);
                }
                
                _debuggableBitmapControl.AddPoints(intersectionPoints);
            }

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
        PointInfo.IsInside = InsideChecker.IsInside(point, Poly.EdgePointsToPixels());
    }

    private void UpdateImage()
    {
        TargetImage?.InvalidateVisual();
    }

    private void Redraw()
    {
        List<Pixel> pixels = [];
        pixels.AddRange(Poly.Draw());
        if (AutoNorms) pixels.AddRange(Poly.DrawNorms());
        ConvexResult = Poly.Convex();

        _debuggableBitmapControl.ClearBitmap(true);
        _debuggableBitmapControl.AddPoints(pixels);
    }

    [RelayCommand]
    public void ClearBitmap()
    {
        Waypoints.Clear();
        Poly.Clear();
        _debuggableBitmapControl.ClearBitmap();
    }

    [RelayCommand]
    private void HandleDebugNextStep()
    {
        _debuggableBitmapControl.HandleBulk(10);
    }
}