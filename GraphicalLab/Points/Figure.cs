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

    public List<Pixel> Draw()
    {
        List<Pixel> pixels = [];
        foreach (var point in Points)
        {
            pixels.Add(point.Perspective());
        }

        foreach (var line in Lines)
        {
            var startPoint = line.StartPoint.Perspective();
            var endPoint = line.EndPoint.Perspective();
            
            pixels.AddRange(XiaolinWuLineGenerator.DrawLine(startPoint, endPoint));
        }
        
        return pixels;
    }
}