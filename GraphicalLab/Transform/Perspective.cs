using System.Collections.Generic;
using GraphicalLab.Points;

namespace GraphicalLab.Transform;

public class Perspective
{
    private double _cameraDistance;
    private double _focalLength;

    public Perspective(double cameraDistance = 100, double focalLength = 40)
    {
        _cameraDistance = cameraDistance;
        _focalLength = focalLength;
    }

    public Point3 ProjectPoint(Point3 point)
    {
        double zRelative = _cameraDistance - point.Z;
        if (zRelative < 10) zRelative = 10;
        
        double scale = _focalLength / zRelative;
        
        return new Point3(
            point.X * scale,
            point.Y * scale,
            point.Z
        );
    }

    public Figure ApplyPerspective(Figure initial)
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

        List<Point3> newPoints = new List<Point3>(collectedPoints.Count);
        foreach (var point in collectedPoints)
        {
            newPoints.Add(ProjectPoint(point));
        }

        return new Figure(newPoints, pointsCount, linesCount);
    }
}