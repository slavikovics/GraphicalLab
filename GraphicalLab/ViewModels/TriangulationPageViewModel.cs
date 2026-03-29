using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GraphicalLab.Controls.WaypointControl;
using GraphicalLab.Services.DebugControlService;
using GraphicalLab.Services.ToastManagerService;
using GraphicalLab.Triangulation;
using GraphicalLab.Voronoi;
using Point = Avalonia.Point;

namespace GraphicalLab.ViewModels;

public partial class TriangulationPageViewModel : ViewModelBase
{
    private readonly IToastManager _toastManager;
    private readonly IDebuggableBitmapControl _debuggableBitmapControl;
    private IDrawable? _drawable;

    public int BitmapWidth => _debuggableBitmapControl.GetBitmapWidth();
    public int BitmapHeight => _debuggableBitmapControl.GetBitmapHeight();
    public WriteableBitmap Bitmap => _debuggableBitmapControl.GetBitmap();
    public Image? TargetImage = null;

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

    public ObservableCollection<WaypointModel> Waypoints { get; } = [];

    public TriangulationPageViewModel(IToastManager toastManager, IDebuggableBitmapControl debuggableBitmapControl)
    {
        _toastManager = toastManager;
        _debuggableBitmapControl = debuggableBitmapControl;
        _debuggableBitmapControl.WritableBitmapChanged += UpdateImage;
        _debuggableBitmapControl.PropertyChanged += DebuggableBitmapControlOnPropertyChanged;
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
        IsNextStepAvailable = _debuggableBitmapControl.IsNextStepAvailable;
        IsDebugEnabled = _debuggableBitmapControl.IsDebugEnabled;
        StepsCountText = _debuggableBitmapControl.StepsCountText;
    }

    [RelayCommand]
    private void Triangulation()
    {
        _drawable = new TriangulationResult();
        Redraw();
    }

    [RelayCommand]
    private void Voronoi()
    {
        _drawable = new VoronoiResult();
        Redraw();
    }

    [RelayCommand]
    private void AddWaypoint(Point center)
    {
        var newWaypoint = new WaypointModel { X = center.X, Y = center.Y };
        Waypoints.Add(newWaypoint);
        Redraw();
    }

    [RelayCommand]
    private void WaypointClicked(WaypointModel? model)
    {
        Redraw();
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

    private void Redraw()
    {
        _debuggableBitmapControl.ClearBitmap(true);
        var canvasSize = new Size(_debuggableBitmapControl.GetBitmapWidth(),
            _debuggableBitmapControl.GetBitmapHeight());
        var points = WaypointModel.ToPixels(Waypoints.ToList(), canvasSize);
        var pixels = _drawable?.Draw(points);
        if (pixels != null) _debuggableBitmapControl.AddPoints(pixels);
    }

    [RelayCommand]
    private void ClearBitmap()
    {
        Waypoints.Clear();
        _debuggableBitmapControl.ClearBitmap();
    }

    [RelayCommand]
    private void HandleDebugNextStep()
    {
        _debuggableBitmapControl.HandleBulk(10);
    }
}