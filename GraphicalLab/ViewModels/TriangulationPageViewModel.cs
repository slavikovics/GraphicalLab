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

public partial class TriangulationPageViewModel : ViewModelBase
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

    public ObservableCollection<WaypointModel> Waypoints { get; } = [];
    public Poly.Poly Poly;

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
        Poly = new Poly.Poly(new Size(_debuggableBitmapControl.GetBitmapWidth(),
            _debuggableBitmapControl.GetBitmapHeight()));
        IsNextStepAvailable = _debuggableBitmapControl.IsNextStepAvailable;
        IsDebugEnabled = _debuggableBitmapControl.IsDebugEnabled;
        StepsCountText = _debuggableBitmapControl.StepsCountText;
    }

    [RelayCommand]
    private void Triangulation()
    {
        
    }

    [RelayCommand]
    private void Voronoi()
    {
        
    }

    [RelayCommand]
    private void AddWaypoint(Point center)
    {
        var newWaypoint = new WaypointModel { X = center.X, Y = center.Y };
        Waypoints.Add(newWaypoint);
        Poly.AddPoint(newWaypoint);
        Redraw();
    }

    [RelayCommand]
    private void WaypointClicked(WaypointModel? model)
    {
        Poly.Close(model);
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

    public void Redraw()
    {
        List<Pixel> pixels = [];
        pixels.AddRange(Poly.Draw());

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