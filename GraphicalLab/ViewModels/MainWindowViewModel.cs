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
    private readonly IWritableBitmapProvider _writableBitmapProvider;
    public ISukiToastManager ToastManager => _toastManager.GetToastManager();

    [ObservableProperty] private List<string> _lineTypes = ["ЦДА", "Брезенхем", "Ву"];

    private delegate List<Pixel> DrawLineDelegate(Pixel start, Pixel end, uint color);
    private Dictionary<int, DrawLineDelegate> _lineTypesMatch = null!;
    public Image? TargetImage = null;

    public int BitmapWidth => _writableBitmapProvider.GetBitmapWidth();
    public int BitmapHeight => _writableBitmapProvider.GetBitmapHeight();

    [ObservableProperty] private bool _isGridVisible = true;
    [ObservableProperty] private int _selectedLineIndex;
    [ObservableProperty] private bool _isNextStepAvailable;

    public WriteableBitmap Bitmap => _writableBitmapProvider.GetBitmap();
    private readonly List<Pixel> _pointsToDraw = [];
    private Pixel? _firstPoint;

    public string StepsCountText
    {
        get;
        set => SetProperty(ref field, value);
    }

    public bool IsDebugEnabled
    {
        get;
        set
        {
            SetProperty(ref field, value);
            if (!value) DrawAllPoints();
            IsNextStepAvailable = value && _pointsToDraw.Count != 0;
            StepsCountText = $"({_pointsToDraw.Count.ToString()})";
        }
    }

    public MainWindowViewModel(IToastManager toastManager, IWritableBitmapProvider writableBitmapProvider)
    {
        _toastManager = toastManager;
        _writableBitmapProvider = writableBitmapProvider;
        InitializeLines();
        StepsCountText = "(0)";
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

        _toastManager.ShowToast("Mouse Clicked", $"X: {x}, Y: {y}", NotificationType.Information);

        if (_firstPoint is null)
        {
            _firstPoint = new Pixel(x, y);
            _writableBitmapProvider.SetPixel(new Pixel(x, y));
        }
        else
        {
            _writableBitmapProvider.SetPixel(new Pixel(x, y));
            var points = _lineTypesMatch[SelectedLineIndex].Invoke(_firstPoint, new Pixel(x, y), 0xFF0000FF);
            AddPoints(points);
            _firstPoint = null;
        }

        TargetImage?.InvalidateVisual();
    }

    private void DrawAllPoints()
    {
        foreach (var newPoint in _pointsToDraw)
        {
            if (newPoint.Intensity != 0) _writableBitmapProvider.SetPixel(newPoint);
        }

        TargetImage?.InvalidateVisual();
        _pointsToDraw.Clear();
    }

    private void AddPoints(List<Pixel> points)
    {
        if (points.Count == 0) return;
        _pointsToDraw.AddRange(points);
        if (IsDebugEnabled)
        {
            IsNextStepAvailable = true;
            StepsCountText = $"({_pointsToDraw.Count.ToString()})";
        }
        else
        {
            DrawAllPoints();
        }
    }

    private List<Pixel> DrawLineDda(Pixel start, Pixel end, uint color = 0xFF0000FF)
    {
        return DdaLineGenerator.DrawLine(start, end, color);
    }

    private List<Pixel> DrawLineBrezenhem(Pixel start, Pixel end, uint color = 0xFF0000FF)
    {
        return BrezenhemLineGenerator.DrawLine(start, end, color);
    }

    private List<Pixel> DrawLineWu(Pixel start, Pixel end, uint color = 0xFF0000FF)
    {
        return XiaolinWuLineGenerator.DrawLine(start, end, color);
    }

    public unsafe void ClearBitmap(Image image)
    {
        _pointsToDraw.Clear();
        IsNextStepAvailable = false;
        StepsCountText = $"({_pointsToDraw.Count.ToString()})";
        _writableBitmapProvider.ClearBitmap(image);
    }

    [RelayCommand]
    private void HandleDebugNextStep()
    {
        if (!IsDebugEnabled || _pointsToDraw.Count == 0) return;
        _writableBitmapProvider.SetPixel(_pointsToDraw[0]);
        TargetImage?.InvalidateVisual();
        _pointsToDraw.RemoveAt(0);
        StepsCountText = $"({_pointsToDraw.Count.ToString()})";
        if (_pointsToDraw.Count == 0) IsNextStepAvailable = false;
    }
}