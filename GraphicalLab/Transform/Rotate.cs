using System;
using System.Collections.Generic;
using GraphicalLab.Matrix;
using GraphicalLab.Points;

namespace GraphicalLab.Transform;

public class Rotate
{
    private readonly Matrix<double> _matrixX;
    private readonly Matrix<double> _matrixY;
    private readonly Matrix<double> _matrixZ;

    public Rotate()
    {
        _matrixX = new Matrix<double>(4, 4);
        _matrixY = new Matrix<double>(4, 4);
        _matrixZ = new Matrix<double>(4, 4);

        _matrixX.SetValue(0, 0, 1);
        _matrixX.SetValue(3, 3, 1);

        _matrixY.SetValue(1, 1, 1);
        _matrixY.SetValue(3, 3, 1);

        _matrixZ.SetValue(2, 2, 1);
        _matrixZ.SetValue(3, 3, 1);
    }

    private Point3 TranslateMatrixToPoint3(Matrix<double> rotated)
    {
        var x = rotated.GetValue(0, 0);
        var y = rotated.GetValue(1, 0);
        var z = rotated.GetValue(2, 0);
        var w = rotated.GetValue(3, 0);
        return new Point4(x, y, z, w).ToPoint3();
    }

    private List<Point3> RotateX(List<Point3> points, double angle)
    {
        _matrixX.SetValue(1, 1, Math.Cos(angle));
        _matrixX.SetValue(1, 2, Math.Sin(angle));
        _matrixX.SetValue(2, 1, -1 * Math.Sin(angle));
        _matrixX.SetValue(2, 2, Math.Cos(angle));

        List<Point3> result = new List<Point3>();

        foreach (var point in points)
        {
            var pointMatrix = point.ToVector();
            var rotated = _matrixX * pointMatrix;
            result.Add(TranslateMatrixToPoint3(rotated));
        }

        return result;
    }

    private List<Point3> RotateY(List<Point3> points, double angle)
    {
        _matrixY.SetValue(0, 0, Math.Cos(angle));
        _matrixY.SetValue(0, 2, -1 * Math.Sin(angle));
        _matrixY.SetValue(2, 0, Math.Sin(angle));
        _matrixY.SetValue(2, 2, Math.Cos(angle));

        List<Point3> result = new List<Point3>();

        foreach (var point in points)
        {
            var pointMatrix = point.ToVector();
            var rotated = _matrixY * pointMatrix;
            result.Add(TranslateMatrixToPoint3(rotated));
        }

        return result;
    }

    private List<Point3> RotateZ(List<Point3> points, double angle)
    {
        _matrixZ.SetValue(0, 0, Math.Cos(angle));
        _matrixZ.SetValue(0, 1, Math.Sin(angle));
        _matrixZ.SetValue(1, 0, -1 * Math.Sin(angle));
        _matrixZ.SetValue(1, 1, Math.Cos(angle));

        List<Point3> result = new List<Point3>();

        foreach (var point in points)
        {
            var pointMatrix = point.ToVector();
            var rotated = _matrixZ * pointMatrix;
            result.Add(TranslateMatrixToPoint3(rotated));
        }

        return result;
    }

    public Figure RotateFigure(Figure initial, double angle, Direction direction)
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

        List<Point3> newPoints;

        switch (direction)
        {
            case Direction.X:
                newPoints = RotateX(collectedPoints, angle);
                break;

            case Direction.Y:
                newPoints = RotateY(collectedPoints, angle);
                break;

            case Direction.Z:
                newPoints = RotateZ(collectedPoints, angle);
                break;

            default:
                newPoints = [];
                break;
        }

        return new Figure(newPoints, pointsCount, linesCount);
    }
}