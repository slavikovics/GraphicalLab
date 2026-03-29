namespace GraphicalLab.Voronoi;

public class Edge
{
    public double A { get; set; }
    public double B { get; set; }
    public double C { get; set; }
    public Site?[] Region { get; } = new Site?[2];
    public Site?[] Endpoint { get; } = new Site?[2];
    public int Index { get; set; }
}