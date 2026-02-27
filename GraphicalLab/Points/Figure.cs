using System;
using System.Collections.Generic;
using GraphicalLab.Lines;
using GraphicalLab.Models;

namespace GraphicalLab.Points;

public class Figure
{
    public List<Point3> Points { get; set; }
    public List<Line> Lines { get; set; }

    public Figure(List<Point3> points, List<Line> lines)
    {
        Points = points;
        Lines = lines;
    }

    public Figure()
    {
        Points = [];
        Lines = [];
    }

    public Figure(List<Point3> points, int pointsCount, int linesCount)
    {
        var totalCount = pointsCount + linesCount * 2;
        if (points.Count != totalCount)
        {
            throw new ArgumentException("Points count must be equal to points count + linesCount * 2");
        }

        Points = [];
        Lines = [];

        for (int i = 0; i < pointsCount; i++)
        {
            Points.Add(points[i]);
        }

        for (int i = pointsCount; i < totalCount - 1; i += 2)
        {
            Lines.Add(new Line(points[i], points[i + 1]));
        }
    }

    public List<Pixel> Draw()
    {
        List<Pixel> pixels = [];


        foreach (var point in Points)
        {
            pixels.Add(point.Perspective());
        }


        foreach (var line in Lines)
        {
            var startPoint = line.StartPoint.Ortagonal();
            var endPoint = line.EndPoint.Ortagonal();

            pixels.AddRange(XiaolinWuLineGenerator.DrawLine(startPoint, endPoint));
        }

        return pixels;
    }
}