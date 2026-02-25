using System.Collections.Generic;

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
}