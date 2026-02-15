using System;
using System.Collections.Generic;
using Avalonia;
using GraphicalLab.Matrix;
using GraphicalLab.Models;

namespace GraphicalLab.Curves;

public class Ermit : ICurveGenerator
{
    private readonly Matrix<double> _ermitMatrix;
    
    public Ermit()
    {
        _ermitMatrix = new Matrix<double>(4, 4);
        
        _ermitMatrix.SetValue(0, 0, 2);
        _ermitMatrix.SetValue(0, 1, -2);
        _ermitMatrix.SetValue(0, 2, 1);
        _ermitMatrix.SetValue(0, 3, 1);
        
        _ermitMatrix.SetValue(1, 0, -3);
        _ermitMatrix.SetValue(1, 1, 3);
        _ermitMatrix.SetValue(1, 2, -2);
        _ermitMatrix.SetValue(1, 3, -1);
        
        _ermitMatrix.SetValue(2, 0, 0);
        _ermitMatrix.SetValue(2, 1, 0);
        _ermitMatrix.SetValue(2, 2, 1);
        _ermitMatrix.SetValue(2, 3, 0);
        
        _ermitMatrix.SetValue(3, 0, 1);
        _ermitMatrix.SetValue(3, 1, 0);
        _ermitMatrix.SetValue(3, 2, 0);
        _ermitMatrix.SetValue(3, 3, 0);
    }

    private Matrix<double> GenerateGeometryMatrix(Point p1, Point p4, Point r1, Point r4)
    {
        var vector = new Matrix<double>(4, 2);
        
        vector.SetValue(0, 0, p1.X);
        vector.SetValue(0, 1, p1.Y);
        
        vector.SetValue(1, 0, p4.X);
        vector.SetValue(1, 1, p4.Y);
        
        vector.SetValue(2, 0, r1.X);
        vector.SetValue(2, 1, r1.Y);
                
        vector.SetValue(3, 0, r4.X);
        vector.SetValue(3, 1, r4.Y);
        
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
        var uniquePixels = new HashSet<(int X, int Y)>();
    
        var p1 = waypoints[0];
        var p2 = waypoints[1];
        var p3 = waypoints[2];
        var p4 = waypoints[3];

        var r1 = 3 * (p2 - p1);
        var r4 = 3 * (p4 - p3);

        var geometryMatrix = GenerateGeometryMatrix(p1, p4, r1, r4);

        var ermitTimesGeometry = _ermitMatrix * geometryMatrix;

        for (double t = 0; t <= 1; t += 0.001)
        {
            var vector = GenerateTVector(t);
            var result = vector * ermitTimesGeometry;
        
            int x = (int)Math.Round(result.GetValue(0, 0));
            int y = (int)Math.Round(result.GetValue(0, 1));
        
            if (uniquePixels.Add((x, y)))
            {
                pixels.Add(new Pixel(x, y, color));
            }
        }
    
        return pixels;
    }
}