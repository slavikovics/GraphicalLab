using System;
using GraphicalLab.Matrix;
using GraphicalLab.Models;

namespace GraphicalLab.Points;

public class Point3
{
    private const double Tolerance = 0.03;
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }

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

    public Matrix<double> ToVector()
    {
        var matrix = new Matrix<double>(4, 1);
        matrix.SetValue(0, 0, X);
        matrix.SetValue(1, 0, Y);
        matrix.SetValue(2, 0, Z);
        matrix.SetValue(3, 0, 1);
        return matrix;
    }

    public Pixel Ortagonal()
    {
        return new Pixel((int)Math.Round(X), (int)Math.Round(Y));
    }
}