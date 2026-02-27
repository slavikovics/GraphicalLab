using System;
using System.Collections.Generic;
using GraphicalLab.Matrix;
using GraphicalLab.Points;

namespace GraphicalLab.Transform;

public class Move
{
    private readonly Matrix<double> _moveMatrix;

    public Move()
    {
        _moveMatrix = new Matrix<double>(4, 4);

        _moveMatrix.SetValue(0, 0, 1);
        _moveMatrix.SetValue(1, 1, 1);
        _moveMatrix.SetValue(2, 2, 1);
        _moveMatrix.SetValue(3, 3, 1);
    }

    private Point3 TranslateMatrixToPoint3(Matrix<double> rotated)
    {
        var x = rotated.GetValue(0, 0);
        var y = rotated.GetValue(1, 0);
        var z = rotated.GetValue(2, 0);
        var w = rotated.GetValue(3, 0);
        return new Point4(x, y, z, w).ToPoint3();
    }
    
    private List<Point3> MovePoints(List<Point3> points, double dx, double dy, double dz)
    {
        _moveMatrix.SetValue(0, 3, dx);
        _moveMatrix.SetValue(1, 3, dy);
        _moveMatrix.SetValue(2, 3, dz);

        List<Point3> result = [];

        foreach (var point in points)
        {
            var pointMatrix = point.ToVector();
            var moved = _moveMatrix * pointMatrix;
            result.Add(TranslateMatrixToPoint3(moved));
        }

        return result;
    }

    public Figure MoveFigure(Figure initial, double dx, double dy, double dz)
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

        var newPoints = MovePoints(collectedPoints, dx, dy, dz);

        return new Figure(newPoints, pointsCount, linesCount);
    }
}