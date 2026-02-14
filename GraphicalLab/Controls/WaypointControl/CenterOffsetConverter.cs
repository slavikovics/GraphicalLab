using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace GraphicalLab.Controls.WaypointControl;

public class CenterOffsetConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is double pos && double.TryParse(parameter?.ToString(), out var half) 
            ? pos - half 
            : AvaloniaProperty.UnsetValue;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is double pos && double.TryParse(parameter?.ToString(), out var half) 
            ? pos + half 
            : AvaloniaProperty.UnsetValue;
}