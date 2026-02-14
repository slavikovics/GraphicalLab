using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace GraphicalLab.Controls.WaypointControl;

public class RelativeToAbsoluteConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count >= 2 && 
            values[0] is double relativeCoord && 
            values[1] is double canvasSize)
        {
            var offset = parameter as double? ?? 0;
            relativeCoord = Math.Max(0, Math.Min(1, relativeCoord));
            
            return (relativeCoord * canvasSize) - offset;
        }
        return 0.0;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}