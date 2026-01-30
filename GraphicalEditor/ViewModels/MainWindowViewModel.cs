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
using SukiUI.Toasts;

namespace GraphicalEditor.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private int _bitmapWidth = 300;
    [ObservableProperty] private int _bitmapHeight = 300;
    private const int DebugStep = 1;
    public Image? TargetImage = null;

    public ISukiToastManager ToastManager { get; } = new SukiToastManager();
    [ObservableProperty] private List<string> _lineTypes = ["ЦДА", "Брезенхем", "Ву"];
    [ObservableProperty] private bool _isGridVisible = true;
    [ObservableProperty] private int _selectedLineIndex;
    [ObservableProperty] private WriteableBitmap _bitmap;

    private readonly List<Point> _pointsToDraw = [];

    public string StepsCountText
    {
        get => field;
        set => SetProperty(ref field, value);
    }

    public bool IsDebugEnabled
    {
        set
        {
            SetProperty(ref field, value);
            if (!value) DrawAllPoints();
            IsNextStepAvailable = value && _pointsToDraw.Count != 0;
            StepsCountText = $"({_pointsToDraw.Count.ToString()})";
        }
        get;
    }

    [ObservableProperty] private bool _isNextStepAvailable;

    private Point? _firstPoint;

    public MainWindowViewModel()
    {
        _bitmap = new(
            new PixelSize(_bitmapWidth, _bitmapHeight),
            new Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Premul);
        StepsCountText = "(0)";
    }

    [RelayCommand]
    private void HandleClick(PointerPressedEventArgs e)
    {
        var point = e.GetPosition(TargetImage);
        if (TargetImage is null) return;
        double scale = TargetImage.Bounds.Width / BitmapWidth;
        int x = (int)(point.X / scale);
        int y = (int)(point.Y / scale);

        ShowToast("Mouse Clicked", $"X: {x}, Y: {y}", NotificationType.Information);

        if (_firstPoint is null)
        {
            _firstPoint = new Point(x, y);
            SetPixel(x, y);
        }
        else
        {
            var points = DrawLineBrezenhem(_firstPoint.Value, new Point(x, y));
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
            SetPixel((int)newPoint.X, (int)newPoint.Y);
        }

        TargetImage?.InvalidateVisual();
        _pointsToDraw.Clear();
    }

    private void AddPoints(List<Point> points)
    {
        if (points.Count == 0) return;
        _pointsToDraw.AddRange(points);
        if (IsDebugEnabled)
        {
            IsNextStepAvailable = true;
            StepsCountText = $"({_pointsToDraw.Count.ToString()})";
        }
    }

    public unsafe void SetPixel(int x, int y, uint color = 0xFF0000FF)
    {
        if (x < 0 || x >= BitmapWidth || y < 0 || y >= BitmapHeight)
            return;

        using var fb = Bitmap.Lock();
        uint* buffer = (uint*)fb.Address;
        int stride = fb.RowBytes / 4;

        buffer[y * stride + x] = color;
    }

    public void DrawLineDda(Point start, Point end, uint color = 0xFF0000FF)
    {
        double dx = end.X - start.X;
        double dy = end.Y - start.Y;

        int steps = (int)Math.Max(Math.Abs(dx), Math.Abs(dy));
        if (steps == 0)
        {
            SetPixel((int)start.X, (int)start.Y, color);
            return;
        }

        double xIncrement = dx / steps;
        double yIncrement = dy / steps;

        double x = start.X;
        double y = start.Y;

        for (int i = 0; i <= steps; i++)
        {
            SetPixel((int)Math.Round(x), (int)Math.Round(y), color);
            x += xIncrement;
            y += yIncrement;
        }
    }

    public List<Point> DrawLineBrezenhem(Point start, Point end, uint color = 0xFF0000FF)
    {
        List<Point> newPoints = [];
        int x1 = (int)start.X, y1 = (int)start.Y;
        int x2 = (int)end.X, y2 = (int)end.Y;

        int dx = Math.Abs(x2 - x1);
        int dy = Math.Abs(y2 - y1);

        int sx = (x1 < x2) ? 1 : -1;
        int sy = (y1 < y2) ? 1 : -1;

        int err = dx - dy;

        while (true)
        {
            newPoints.Add(new Point(x1, y1));
            if (x1 == x2 && y1 == y2) break;

            int e2 = 2 * err;

            if (e2 > -dy)
            {
                err -= dy;
                x1 += sx;
            }

            if (e2 < dx)
            {
                err += dx;
                y1 += sy;
            }
        }

        return newPoints;
    }

    private void ShowToast(string title, string content, NotificationType notificationType)
    {
        var builder = ToastManager.CreateToast();
        builder.SetContent(content);
        builder.SetTitle(title);
        builder.SetType(notificationType);
        builder.SetCanDismissByClicking(true);
        builder.SetDismissAfter(TimeSpan.FromSeconds(2));
        builder.Queue();
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
        SetPixel((int)_pointsToDraw[0].X, (int)_pointsToDraw[0].Y);
        TargetImage?.InvalidateVisual();
        _pointsToDraw.RemoveAt(0);
        StepsCountText = $"({_pointsToDraw.Count.ToString()})";
        if (_pointsToDraw.Count == 0)
        {
            IsNextStepAvailable = false;
        }
    }
}