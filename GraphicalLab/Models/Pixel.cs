using System;

namespace GraphicalLab;

public class Pixel
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public double Intensity { get; private set; }
    public uint Color { get; private set; }

    public Pixel(int x, int y, uint color = 0xFF0000FF, double intensity = 1)
    {
        X = x;
        Y = y;
        Intensity = 1;
        Color = ApplyIntensityToColor(color, intensity);
    }

    public Pixel(double x, double y, uint color = 0xFF0000FF, double intensity = 1)
    {
        X = (int)x;
        Y = (int)y;
        Intensity = 1;
        Color = ApplyIntensityToColor(color, intensity);
    }

    private uint ApplyIntensityToColor(uint color, double intensity)
    {
        intensity = Math.Clamp(intensity, 0.0, 1.0);

        byte a = (byte)((color >> 24) & 0xFF);
        byte r = (byte)((color >> 16) & 0xFF);
        byte g = (byte)((color >> 8) & 0xFF);
        byte b = (byte)(color & 0xFF);

        double factor = intensity;

        byte newA = (byte)(a * factor);
        byte newR = (byte)(r * factor);
        byte newG = (byte)(g * factor);
        byte newB = (byte)(b * factor);

        return ((uint)newA << 24)
               | ((uint)newR << 16)
               | ((uint)newG << 8)
               | newB;
    }
    
    public Pixel Swapped()
    {
        return new Pixel(Y, X, Color, Intensity);
    }
    
    public void Swap()
    {
        (X, Y) = (Y, X);
    }

    public override string ToString()
    {
        return $"({X},{Y})";
    }
}