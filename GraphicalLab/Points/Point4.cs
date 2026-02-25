using System;

namespace GraphicalLab.Points;

public class Point4
{
    private const double Tolerance = 0.03;
    public readonly double X;
    public readonly double Y;
    public readonly double Z;
    public readonly double W;

    public Point4(double x, double y, double z, double w)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    public static bool operator ==(Point4 left, Point4 right)
    {
        return Math.Abs(left.X - right.X) < Tolerance && 
               Math.Abs(left.Y - right.Y) < Tolerance && 
               Math.Abs(left.Z - right.Z) < Tolerance && 
               Math.Abs(left.W - right.W) < Tolerance;
    }

    public static bool operator !=(Point4 left, Point4 right)
    {
        return !(left == right);
    }

    private bool Equals(Point4 other)
    {
        return this == other;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Point3)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z, W);
    }

    public Point3 ToPoint3()
    {
        return new Point3(X / W, Y / W, Z / W);
    }
}