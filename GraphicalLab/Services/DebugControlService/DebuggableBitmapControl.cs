using System;
using System.Collections.Generic;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using GraphicalLab.Services.WritableBitmapProviderService;

namespace GraphicalLab.Services.DebugControlService;

public partial class DebuggableBitmapControl : ObservableObject, IDebuggableBitmapControl
{
    private readonly IWritableBitmapProvider _writableBitmapProvider;

    private List<Pixel> PointsToDraw { get; }
    public event Action WritableBitmapChanged;
    [ObservableProperty] private bool _isNextStepAvailable;
    [ObservableProperty] private bool _isGridVisible;

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
            IsNextStepAvailable = value && PointsToDraw.Count != 0;
            StepsCountText = $"({PointsToDraw.Count.ToString()})";
        }
    }

    public DebuggableBitmapControl(IWritableBitmapProvider writableBitmapProvider)
    {
        PointsToDraw = [];
        _writableBitmapProvider = writableBitmapProvider;
        StepsCountText = "(0)";
    }

    public void AddPoints(List<Pixel> points)
    {
        if (points.Count == 0) return;
        foreach (var point in points)
        {
            if (!(point.X >= GetBitmapWidth() || point.Y >= GetBitmapHeight()))
            {
                PointsToDraw.Add(point);
            }
        }

        if (IsDebugEnabled)
        {
            IsNextStepAvailable = true;
            StepsCountText = $"({PointsToDraw.Count.ToString()})";
        }
        else
        {
            DrawAllPoints();
        }
    }

    public void ClearBitmap()
    {
        PointsToDraw.Clear();
        IsNextStepAvailable = false;
        StepsCountText = $"({PointsToDraw.Count.ToString()})";
        _writableBitmapProvider.ClearBitmap();
        WritableBitmapChanged?.Invoke();
    }

    public void HandleDebugNextStep()
    {
        if (!IsDebugEnabled || PointsToDraw.Count == 0) return;
        _writableBitmapProvider.SetPixel(PointsToDraw[0]);
        WritableBitmapChanged?.Invoke();

        PointsToDraw.RemoveAt(0);
        StepsCountText = $"({PointsToDraw.Count.ToString()})";
        if (PointsToDraw.Count == 0) IsNextStepAvailable = false;
    }

    public WriteableBitmap GetBitmap()
    {
        return _writableBitmapProvider.GetBitmap();
    }

    public int GetBitmapWidth()
    {
        return _writableBitmapProvider.GetBitmapWidth();
    }

    public int GetBitmapHeight()
    {
        return _writableBitmapProvider.GetBitmapHeight();
    }

    public void SetPixel(Pixel pixel)
    {
        _writableBitmapProvider.SetPixel(pixel);
        WritableBitmapChanged?.Invoke();
    }

    private void DrawAllPoints()
    {
        foreach (var newPoint in PointsToDraw)
        {
            if (newPoint.Intensity != 0) _writableBitmapProvider.SetPixel(newPoint);
        }

        WritableBitmapChanged?.Invoke();
        PointsToDraw.Clear();
    }
}