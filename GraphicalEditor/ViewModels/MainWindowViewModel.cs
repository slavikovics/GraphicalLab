using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using SukiUI.Toasts;

namespace GraphicalEditor.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private const int BitmapWidth = 300;
    private const int BitmapHeight = 300;
    
    public ISukiToastManager ToastManager { get; } = new SukiToastManager();
    [ObservableProperty] private List<string> _lineTypes = ["Цифровой Дифференциальный Анализатор", "Целочисленный Алгоритм Брезенхема", "Алгоритм Ву"];
    [ObservableProperty] private int _selectedLineIndex;
    [ObservableProperty] private WriteableBitmap _bitmap = new(
        new PixelSize(BitmapWidth, BitmapHeight),
        new Vector(96, 96),
        PixelFormat.Bgra8888,
        AlphaFormat.Premul);

    private Point? _firstPoint;

    public void HandleClick(object? sender, Point point)
    {
        if (sender is not Image image)
            return;

        double scale = image.Bounds.Width / BitmapWidth;

        int x = (int)(point.X / scale);
        int y = (int)(point.Y / scale);

        ShowToast(
            "Mouse Clicked",
            $"X: {x}, Y: {y}",
            NotificationType.Information);

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
        double x = start.X;
        double y = start.Y;
        double dx = end.X - start.X;
        double dy = end.Y - start.Y;
        double e = dy / dx - 0.5;
        SetPixel((int)Math.Round(x), (int)Math.Round(y), color);
        int i = 1;

        while (i <= dx)
        {
            if (e >= 0)
            {
                y++;
                e -= 1;
            }

            x++;
            e += dy / dx;
            i++;
            SetPixel((int)Math.Round(x), (int)Math.Round(y), color);
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
}