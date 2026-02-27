using System;
using System.Collections.Generic;
using GraphicalLab.Matrix;
using GraphicalLab.Points;

namespace GraphicalLab.Transform;

public class Perspective
{
    private double _cameraDistance; // Расстояние от камеры до центра сцены
    private double _focalLength;    // Фокусное расстояние (аналог K)

    public Perspective(double cameraDistance = 500, double focalLength = 200)
    {
        _cameraDistance = cameraDistance;
        _focalLength = focalLength;
    }

    public Point3 ProjectPoint(Point3 point)
    {
        // Координата Z относительно камеры
        // Камера смотрит на центр сцены (0,0,0) с расстояния _cameraDistance
        double zRelative = _cameraDistance - point.Z;
        
        // Защита от слишком маленького Z
        if (zRelative < 10) zRelative = 10;
        
        // Перспективная проекция
        double scale = _focalLength / zRelative;
        
        return new Point3(
            point.X * scale,
            point.Y * scale,
            point.Z // Сохраняем Z для других преобразований
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