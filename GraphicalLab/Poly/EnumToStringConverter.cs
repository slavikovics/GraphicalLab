using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace GraphicalLab.Poly;

public class EnumToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var placeholder = "";
        if (value == null)
            return placeholder;

        switch (value as ConvexResult?)
        {
            case null:
                return placeholder;
            case ConvexResult.AllNegative:
                return placeholder + "Выпуклый";
            case ConvexResult.AllPositive:
                return placeholder + "Выпуклый";
            case ConvexResult.AllZero:
                return placeholder + "Вырожден";
            case ConvexResult.Mixed:
                return placeholder + "Вогнутый";
            default:
                return placeholder + value;
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}