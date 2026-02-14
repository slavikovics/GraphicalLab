using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GraphicalLab.Controls.WaypointControl;
using GraphicalLab.Curves;
using GraphicalLab.Services.DebugControlService;
using GraphicalLab.Services.ToastManagerService;

namespace GraphicalLab.ViewModels;

public partial class CurvesPageViewModel : ViewModelBase
{
    private readonly IToastManager _toastManager;
    private readonly IDebuggableBitmapControl _debuggableBitmapControl;
    private readonly ICurveGenerator _ermitCurveGenerator = new Ermit();
    private readonly ICurveGenerator _bezieCurveGenerator = new Bezie();

    public int BitmapWidth => _debuggableBitmapControl.GetBitmapWidth();
    public int BitmapHeight => _debuggableBitmapControl.GetBitmapHeight();
    public WriteableBitmap Bitmap => _debuggableBitmapControl.GetBitmap();

    public Image? TargetImage = null;

    [ObservableProperty] private bool _isNextStepAvailable;
    [ObservableProperty] private string _stepsCountText;
    [ObservableProperty] private int _selectedCurveIndex;

    [ObservableProperty] private bool _addOnClickEnabled;
    [ObservableProperty] private bool _removeOnClickEnabled = true;
    [ObservableProperty] private bool _connectOnClickEnabled = true;
    [ObservableProperty] private int _selectedWaypointsCount;

    private readonly List<WaypointModel> _selectedWaypoints = [];
    private readonly List<Curve> _curves = [];

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

    private delegate void DrawCurveDelegate();

    private Dictionary<int, DrawCurveDelegate> _curveTypesMatch = null!;
    public ObservableCollection<WaypointModel> Waypoints { get; } = [];

    public CurvesPageViewModel(IToastManager toastManager, IDebuggableBitmapControl debuggableBitmapControl)
    {
        _toastManager = toastManager;
        _debuggableBitmapControl = debuggableBitmapControl;
        _debuggableBitmapControl.WritableBitmapChanged += UpdateImage;
        _debuggableBitmapControl.PropertyChanged += DebuggableBitmapControlOnPropertyChanged;
        InitializeCurves();
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
        SelectedCurveIndex = 0;
        SelectedWaypointsCount = 0;
        IsNextStepAvailable = _debuggableBitmapControl.IsNextStepAvailable;
        IsDebugEnabled = _debuggableBitmapControl.IsDebugEnabled;
        StepsCountText = _debuggableBitmapControl.StepsCountText;
    }

    private void InitializeCurves()
    {
        _curveTypesMatch = new Dictionary<int, DrawCurveDelegate>();
        _curveTypesMatch.Add(0, DrawErmit);
        _curveTypesMatch.Add(1, DrawBezie);
        _curveTypesMatch.Add(2, DrawSpline);
    }

    [RelayCommand]
    private void AddButton()
    {
        AddOnClickEnabled = false;
        RemoveOnClickEnabled = true;
        ConnectOnClickEnabled = true;
        ClearSelectedWaypoints();
    }

    [RelayCommand]
    private void RemoveButton()
    {
        AddOnClickEnabled = true;
        RemoveOnClickEnabled = false;
        ConnectOnClickEnabled = true;
        ClearSelectedWaypoints();
    }

    [RelayCommand]
    private void ConnectButton()
    {
        AddOnClickEnabled = true;
        RemoveOnClickEnabled = true;
        ConnectOnClickEnabled = false;
        _selectedWaypoints.Clear();
    }

    [RelayCommand]
    private void AddWaypoint(Point center)
    {
        if (!AddOnClickEnabled) Waypoints.Add(new WaypointModel { X = center.X, Y = center.Y });
    }

    [RelayCommand]
    private void WaypointClicked(WaypointModel? model)
    {
        if (!RemoveOnClickEnabled && model is not null)
        {
            Waypoints.Remove(model);
            List<Curve> toRemove = [];
            foreach (var curve in _curves)
                if (curve.HasWaypoint(model))
                    toRemove.Add(curve);

            foreach (var curve in toRemove) _curves.Remove(curve);
            Redraw();
        }
        else if (!ConnectOnClickEnabled && model is not null)
        {
            if (_selectedWaypoints.Contains(model))
            {
                _selectedWaypoints.Clear();
                _selectedWaypoints.Add(model);
            }
            else
            {
                _selectedWaypoints.Add(model);
                _curveTypesMatch[SelectedCurveIndex].Invoke();
            }

            SelectedWaypointsCount = _selectedWaypoints.Count;
        }
    }

    [RelayCommand]
    private void WaypointDragged(WaypointModel? model)
    {
        Redraw();
    }

    private void UpdateImage()
    {
        TargetImage?.InvalidateVisual();
    }

    private void DrawErmit()
    {
        var curves = Curve.CreateCurves(_selectedWaypoints, new Size(BitmapWidth, BitmapHeight), _ermitCurveGenerator);
        if (curves != null)
        {
            _curves.AddRange(curves);
            Redraw();
        }
    }

    private void DrawBezie()
    {
        var curves = Curve.CreateCurves(_selectedWaypoints, new Size(BitmapWidth, BitmapHeight), _bezieCurveGenerator);
        if (curves != null)
        {
            _curves.AddRange(curves);
            Redraw();
        }
    }

    private void DrawSpline()
    {
    }

    private void Redraw()
    {
        _debuggableBitmapControl.ClearBitmap();
        foreach (var curve in _curves) _debuggableBitmapControl.AddPoints(curve.Draw());
    }

    private void ClearSelectedWaypoints()
    {
        _selectedWaypoints.Clear();
        SelectedWaypointsCount = _selectedWaypoints.Count;
    }

    [RelayCommand]
    public void ClearBitmap()
    {
        Waypoints.Clear();
        _curves.Clear();
        _debuggableBitmapControl.ClearBitmap();
        ClearSelectedWaypoints();
    }

    [RelayCommand]
    private void HandleDebugNextStep()
    {
        _debuggableBitmapControl.HandleDebugNextStep();
    }
}