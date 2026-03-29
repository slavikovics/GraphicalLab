namespace GraphicalLab.Voronoi;

public class Site
{
    public Point Coordinate { get; }
    public int Index { get; set; }
    public int ReferenceCount { get; set; }
    public Site(Point coordinate) => Coordinate = coordinate;
}