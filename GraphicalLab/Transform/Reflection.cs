using System;
using System.Collections.Generic;
using GraphicalLab.Matrix;
using GraphicalLab.Points;

namespace GraphicalLab.Transform;

public class Reflection
{
    private readonly Matrix<double> _reflectionMatrix;

    public Reflection()
    {
        _reflectionMatrix = new Matrix<double>(4, 4);
        ResetToIdentity();
    }

    private void ResetToIdentity()
    {
        for (int i = 0; i < 4; i++)
        for (int j = 0; j < 4; j++)
            _reflectionMatrix.SetValue(i, j, i == j ? 1.0 : 0.0);
    }

    private Point3 TranslateMatrixToPoint3(Matrix<double> transformed)
    {
        var x = transformed.GetValue(0, 0);
        var y = transformed.GetValue(1, 0);
        var z = transformed.GetValue(2, 0);
        var w = transformed.GetValue(3, 0);
        return new Point4(x, y, z, w).ToPoint3();
    }

    private List<Point3> ReflectPoints(List<Point3> points)
    {
        List<Point3> result = new List<Point3>(points.Count);

        foreach (var point in points)
        {
            var pointMatrix = point.ToVector();
            var reflected = _reflectionMatrix * pointMatrix;
            result.Add(TranslateMatrixToPoint3(reflected));
        }

        return result;
    }

    public Figure ReflectX(Figure initial)
    {
        ResetToIdentity();
        _reflectionMatrix.SetValue(0, 0, -1); // X -> -X
        return ApplyReflection(initial);
    }

    public Figure ReflectY(Figure initial)
    {
        ResetToIdentity();
        _reflectionMatrix.SetValue(1, 1, -1); // Y -> -Y
        return ApplyReflection(initial);
    }

    public Figure ReflectZ(Figure initial)
    {
        ResetToIdentity();
        _reflectionMatrix.SetValue(2, 2, -1); // Z -> -Z
        return ApplyReflection(initial);
    }

    public Figure ReflectXY(Figure initial) // Отражение относительно плоскости XY (Z)
    {
        return ReflectZ(initial);
    }

    public Figure ReflectXZ(Figure initial) // Отражение относительно плоскости XZ (Y)
    {
        return ReflectY(initial);
    }

    public Figure ReflectYZ(Figure initial) // Отражение относительно плоскости YZ (X)
    {
        return ReflectX(initial);
    }

    private Figure ApplyReflection(Figure initial)
    {
        var pointsCount = initial.Points.Count;
        var linesCount = initial.Lines.Count;
        var collectedPoints = new List<Point3>();
        collectedPoints.AddRange(initial.Points);

        foreach (var line in initial.Lines)
        {
            collectedPoints.Add(line.StartPoint);
            collectedPoints.Add(line.EndPoint);
        }

        var newPoints = ReflectPoints(collectedPoints);

        return new Figure(newPoints, pointsCount, linesCount);
    }
}