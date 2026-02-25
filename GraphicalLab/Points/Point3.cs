using System;
using Avalonia;

namespace GraphicalLab.Points;

public class Point3
{
    private const double Tolerance = 0.03;
    public readonly double X;
    public readonly double Y;
    public readonly double Z;

    public Point3(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public static bool operator ==(Point3 left, Point3 right)
    {
        return Math.Abs(left.X - right.X) < Tolerance &&
               Math.Abs(left.Y - right.Y) < Tolerance &&
               Math.Abs(left.Z - right.Z) < Tolerance;
    }

    public static bool operator !=(Point3 left, Point3 right)
    {
        return !(left == right);
    }

    private bool Equals(Point3 other)
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
        return HashCode.Combine(X, Y, Z);
    }

    public Point4 ToPoint4()
    {
        return new Point4(X, Y, Z, 1);
    }
}