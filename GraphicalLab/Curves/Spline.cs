using System;
using System.Collections.Generic;
using Avalonia;
using GraphicalLab.Controls.WaypointControl;
using GraphicalLab.Matrix;
using GraphicalLab.Models;

namespace GraphicalLab.Curves;

public class Spline : ICurveGenerator
{
    private readonly Matrix<double> _splineMatrix;

    public Spline()
    {
        _splineMatrix = new Matrix<double>(4, 4);

        _splineMatrix.SetValue(0, 0, -1);
        _splineMatrix.SetValue(0, 1, 3);
        _splineMatrix.SetValue(0, 2, -3);
        _splineMatrix.SetValue(0, 3, 1);

        _splineMatrix.SetValue(1, 0, 3);
        _splineMatrix.SetValue(1, 1, -6);
        _splineMatrix.SetValue(1, 2, 3);
        _splineMatrix.SetValue(1, 3, 0);

        _splineMatrix.SetValue(2, 0, -3);
        _splineMatrix.SetValue(2, 1, 0);
        _splineMatrix.SetValue(2, 2, 3);
        _splineMatrix.SetValue(2, 3, 0);

        _splineMatrix.SetValue(3, 0, 1);
        _splineMatrix.SetValue(3, 1, 4);
        _splineMatrix.SetValue(3, 2, 1);
        _splineMatrix.SetValue(3, 3, 0);
    }

    private Matrix<double> GenerateGeometryMatrix(Point p1, Point p2, Point p3, Point p4)
    {
        var vector = new Matrix<double>(4, 2);

        vector.SetValue(0, 0, p1.X);
        vector.SetValue(0, 1, p1.Y);

        vector.SetValue(1, 0, p2.X);
        vector.SetValue(1, 1, p2.Y);

        vector.SetValue(2, 0, p3.X);
        vector.SetValue(2, 1, p3.Y);

        vector.SetValue(3, 0, p4.X);
        vector.SetValue(3, 1, p4.Y);

        return vector;
    }

    private Matrix<double> GenerateTVector(double t)
    {
        var vector = new Matrix<double>(1, 4);

        vector.SetValue(0, 0, Math.Pow(t, 3));
        vector.SetValue(0, 1, Math.Pow(t, 2));
        vector.SetValue(0, 2, t);
        vector.SetValue(0, 3, 1);

        return vector;
    }

    public List<Pixel> Draw(List<Point> waypoints, uint color = 4278190335)
    {
        var pixels = new List<Pixel>();

        for (int i = 1; i <= waypoints.Count - 3; i++)
        {
            var p1 = waypoints[i - 1];
            var p2 = waypoints[i];
            var p3 = waypoints[i + 1];
            var p4 = waypoints[i + 2];

            var geometryMatrix = GenerateGeometryMatrix(p1, p2, p3, p4);

            for (double t = 0; t <= 1; t += 0.001)
            {
                var vector = GenerateTVector(t);
                var result = 1 / 6.0 * vector * _splineMatrix * geometryMatrix;

                var pixel = new Pixel(result.GetValue(0, 0), result.GetValue(0, 1));
                pixels.Add(pixel);
            }
        }

        return pixels;
    }
}