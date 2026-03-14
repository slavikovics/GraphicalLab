using System.ComponentModel;
using System.Runtime.CompilerServices;
using GraphicalLab.Models;

namespace GraphicalLab.Poly;

public sealed class PointInfo : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public Pixel? Point
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayText));
            }
        }
    }

    public bool IsInside
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayText));
            }
        }
    }

    public string DisplayText
    {
        get
        {
            if (Point == null) 
                return "Точка не выбрана";
            
            string location = IsInside ? "Внутри" : "Снаружи";
            return $"({Point.X}, {Point.Y}) {location}";
        }
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public override string ToString() => DisplayText;
}