using GraphicalLab.Models;

namespace GraphicalLab.Poly;

public struct Vector
{
    public double X { get; set; }
    public double Y { get; set; }

    public Vector(double x, double y)
    {
        X = x;
        Y = y;
    }

    public static Vector operator -(Vector a, Vector b) => 
        new Vector(a.X - b.X, a.Y - b.Y);

    public Pixel ToPixel()
    {
        return new Pixel(X, Y);
    }
    
    public void Invert()
    {
        X = -1 * X;
        Y = -1 * Y;
    }
    
    public override string ToString() => $"({X:F2}, {Y:F2})";
}