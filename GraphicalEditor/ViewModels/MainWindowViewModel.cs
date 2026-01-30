using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
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

    public ISukiToastManager ToastManager { get; } = new SukiToastManager();
    [ObservableProperty] private List<string> _lineTypes = ["ЦДА", "Брезенхем", "Ву"];
    [ObservableProperty] private bool _isGridVisible = true;
    [ObservableProperty] private int _selectedLineIndex;
    [ObservableProperty] private WriteableBitmap _bitmap;

    private Point? _firstPoint;

    public MainWindowViewModel()
    {
        _bitmap = new(
            new PixelSize(_bitmapWidth, _bitmapHeight),
            new Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Premul);
    }

    public void HandleClick(object? sender, Point point)
    {
        if (sender is not Image image)
            return;

        double scale = image.Bounds.Width / BitmapWidth;
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
            DrawLineBrezenhem(_firstPoint.Value, new Point(x, y));
            _firstPoint = null;
        }

        image.InvalidateVisual();
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

    public void DrawLineBrezenhem(Point start, Point end, uint color = 0xFF0000FF)
    {
        int x1 = (int)start.X, y1 = (int)start.Y;
        int x2 = (int)end.X, y2 = (int)end.Y;
    
        int dx = Math.Abs(x2 - x1);
        int dy = Math.Abs(y2 - y1);
    
        int sx = (x1 < x2) ? 1 : -1;
        int sy = (y1 < y2) ? 1 : -1;
    
        int err = dx - dy;
    
        while (true)
        {
            SetPixel(x1, y1, color);
        
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
}