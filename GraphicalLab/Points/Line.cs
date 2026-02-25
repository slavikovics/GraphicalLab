namespace GraphicalLab.Points;

public class Line
{
    public Point3 StartPoint { get; set; }
    public Point3 EndPoint { get; set; }

    public Line(Point3 startPoint, Point3 endPoint)
    {
        StartPoint = startPoint;
        EndPoint = endPoint;
    }
}