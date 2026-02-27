using System.Collections.Generic;
using GraphicalLab.Matrix;
using GraphicalLab.Points;

namespace GraphicalLab.Transform;

public class Scale
{
    private readonly Matrix<double> _scaleMatrix;

    public Scale()
    {
        _scaleMatrix = new Matrix<double>(4, 4);
        _scaleMatrix.SetValue(0, 0, 1);
        _scaleMatrix.SetValue(1, 1, 1);
        _scaleMatrix.SetValue(2, 2, 1);
        _scaleMatrix.SetValue(3, 3, 1);
    }

    private Point3 TranslateMatrixToPoint3(Matrix<double> transformed)
    {
        var x = transformed.GetValue(0, 0);
        var y = transformed.GetValue(1, 0);
        var z = transformed.GetValue(2, 0);
        var w = transformed.GetValue(3, 0);
        return new Point4(x, y, z, w).ToPoint3();
    }
    
    private List<Point3> ScalePoints(List<Point3> points, double sx, double sy, double sz)
    {
        _scaleMatrix.SetValue(0, 0, sx);
        _scaleMatrix.SetValue(1, 1, sy);
        _scaleMatrix.SetValue(2, 2, sz);

        List<Point3> result = new List<Point3>(points.Count);

        foreach (var point in points)
        {
            var pointMatrix = point.ToVector();
            var scaled = _scaleMatrix * pointMatrix;
            result.Add(TranslateMatrixToPoint3(scaled));
        }

        return result;
    }

    public Figure ScaleFigure(Figure initial, double sx, double sy, double sz)
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

        var newPoints = ScalePoints(collectedPoints, sx, sy, sz);

        return new Figure(newPoints, pointsCount, linesCount);
    }
    
    public Figure ScaleFigure(Figure initial, double scale)
    {
        return ScaleFigure(initial, scale, scale, scale);
    }
    
    public Figure ScaleFigureX(Figure initial, double sx)
    {
        return ScaleFigure(initial, sx, 1.0, 1.0);
    }
    
    public Figure ScaleFigureY(Figure initial, double sy)
    {
        return ScaleFigure(initial, 1.0, sy, 1.0);
    }
    
    public Figure ScaleFigureZ(Figure initial, double sz)
    {
        return ScaleFigure(initial, 1.0, 1.0, sz);
    }
}