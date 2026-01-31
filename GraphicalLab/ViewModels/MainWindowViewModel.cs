using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
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
    
    [ObservableProperty] private int _bitmapWidth = 300;
    [ObservableProperty] private int _bitmapHeight = 300;
    
    [ObservableProperty] private bool _isGridVisible = true;
    [ObservableProperty] private int _selectedLineIndex;
    [ObservableProperty] private WriteableBitmap _bitmap;

    private readonly List<Pixel> _pointsToDraw = [];

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

    [ObservableProperty] private bool _isNextStepAvailable;

    private Pixel? _firstPoint;

    public MainWindowViewModel(IToastManager toastManager, IWritableBitmapProvider writableBitmapProvider)
    {
        _toastManager = toastManager;
        _writableBitmapProvider = writableBitmapProvider;
        InitializeLines();
        _bitmap = new(
            new PixelSize(_bitmapWidth, _bitmapHeight),
            new Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Premul);
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
            SetPixel(new Pixel(x, y));
        }
        else
        {
            SetPixel(new Pixel(x, y));
            var points = _lineTypesMatch[SelectedLineIndex].Invoke(_firstPoint, new Pixel(x, y), 0xFF0000FF);
            AddPoints(points);
            UpdateBitmap();
            _firstPoint = null;
        }

        TargetImage?.InvalidateVisual();
    }

    private void UpdateBitmap()
    {
        if (!IsDebugEnabled) DrawAllPoints();
    }

    private void DrawAllPoints()
    {
        foreach (var newPoint in _pointsToDraw)
        {
            if (newPoint.Intensity != 0) SetPixel(newPoint);
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
    }

    public unsafe void SetPixel(Pixel pixel)
    {
        if (pixel.X < 0 || pixel.X >= BitmapWidth || pixel.Y < 0 || pixel.Y >= BitmapHeight)
            return;

        using var fb = Bitmap.Lock();
        uint* buffer = (uint*)fb.Address;
        int stride = fb.RowBytes / 4;

        buffer[pixel.Y * stride + pixel.X] = pixel.Color;
    }

    private List<Pixel> DrawLineDda(Pixel start, Pixel end, uint color = 0xFF0000FF)
    {
        return DdaLineGenerator.DrawLine(start, end, color);
    }

    public List<Pixel> DrawLineBrezenhem(Pixel start, Pixel end, uint color = 0xFF0000FF)
    {
        return BrezenhemLineGenerator.DrawLine(start, end, color);
    }

    public List<Pixel> DrawLineWu(Pixel start, Pixel end, uint color = 0xFF0000FF)
    {
        return XiaolinWuLineGenerator.DrawLine(start, end, color);
    }

    public unsafe void ClearBitmap(Image image)
    {
        _pointsToDraw.Clear();
        IsNextStepAvailable = false;
        StepsCountText = $"({_pointsToDraw.Count.ToString()})";
        using var fb = Bitmap.Lock();
        uint* buffer = (uint*)fb.Address;
        int stride = fb.RowBytes / 4;

        for (int x = 0; x < BitmapWidth; x++)
        {
            for (int y = 0; y < BitmapHeight; y++)
            {
                buffer[y * stride + x] = 0;
            }
        }

        image.InvalidateVisual();
    }

    [RelayCommand]
    private void HandleDebugNextStep()
    {
        if (!IsDebugEnabled || _pointsToDraw.Count == 0) return;
        SetPixel(_pointsToDraw[0]);
        TargetImage?.InvalidateVisual();
        _pointsToDraw.RemoveAt(0);
        StepsCountText = $"({_pointsToDraw.Count.ToString()})";
        if (_pointsToDraw.Count == 0)
        {
            IsNextStepAvailable = false;
        }
    }
}