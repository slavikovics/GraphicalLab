using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GraphicalLab.Controls.WaypointControl;
using GraphicalLab.Curves;
using GraphicalLab.Models;
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

    [ObservableProperty] private List<string> _polyTypes = ["Грэхем", "Джарвис"];

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
        InitializePolys();
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
        _poly = new Poly.Poly(new Size(_debuggableBitmapControl.GetBitmapWidth(),
            _debuggableBitmapControl.GetBitmapHeight()));
        SelectedPolyIndex = 0;
        AddWaypointEnabled = true;
        IsNextStepAvailable = _debuggableBitmapControl.IsNextStepAvailable;
        IsDebugEnabled = _debuggableBitmapControl.IsDebugEnabled;
        StepsCountText = _debuggableBitmapControl.StepsCountText;
    }

    private void InitializePolys()
    {
        _polyTypesMatch = new Dictionary<int, DrawPolyDelegate>();
    }

    [ObservableProperty] private bool _addWaypointEnabled;
    [ObservableProperty] private bool _addLineEnabled;
    [ObservableProperty] private bool _pickPointEnabled;

    [RelayCommand]
    private void AddPoint()
    {
        AddWaypointEnabled = false;
        AddLineEnabled = true;
        PickPointEnabled = true;
    }

    [RelayCommand]
    private void AddLine()
    {
        AddWaypointEnabled = true;
        AddLineEnabled = false;
        PickPointEnabled = true;
    }

    [RelayCommand]
    private void PickPoint()
    {
        AddWaypointEnabled = true;
        AddLineEnabled = true;
        PickPointEnabled = false;
    }

    [RelayCommand]
    private void BuildCapsule()
    {
    }

    [RelayCommand]
    private void AddWaypoint(Point center)
    {
        var newWaypoint = new WaypointModel { X = center.X, Y = center.Y };
        Waypoints.Add(newWaypoint);
        _poly.AddPoint(newWaypoint);
        Redraw();
    }

    [RelayCommand]
    private void WaypointClicked(WaypointModel? model)
    {
        _poly.Close(model);
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
        List<Pixel> pixels = [];
        pixels.AddRange(_poly.Draw());

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