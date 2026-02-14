using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia;

namespace GraphicalLab.Controls.WaypointControl;

public class WaypointModel : INotifyPropertyChanged
{
    public double X
    {
        get;
        set
        {
            field = Math.Max(0, Math.Min(1, value));
            OnPropertyChanged();
        }
    }

    public double Y
    {
        get;
        set
        {
            field = Math.Max(0, Math.Min(1, value));
            OnPropertyChanged();
        }
    }

    public static WaypointModel FromAbsolute(Point absolutePos, Size canvasSize)
    {
        return new WaypointModel
        {
            X = canvasSize.Width > 0 ? absolutePos.X / canvasSize.Width : 0,
            Y = canvasSize.Height > 0 ? absolutePos.Y / canvasSize.Height : 0
        };
    }

    public Point GetAbsolutePosition(Size canvasSize)
    {
        return new Point(
            X * canvasSize.Width,
            Y * canvasSize.Height
        );
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}