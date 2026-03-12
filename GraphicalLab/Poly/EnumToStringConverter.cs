using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace GraphicalLab.Poly;

public class EnumToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return string.Empty;

        switch (value as ConvexResult?)
        {
            case null:
                return string.Empty;
            case ConvexResult.AllNegative:
                return "Выпуклый";
            case ConvexResult.AllPositive:
                return "Выпуклый";
            case ConvexResult.AllZero:
                return "Выроожден";
            case ConvexResult.Mixed:
                return "Вогнутый";
            default:
                return value.ToString();
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}