namespace GraphicalLab.Voronoi;

public class HalfEdge
{
    public HalfEdge? Left { get; set; }
    public HalfEdge? Right { get; set; }
    public Edge? Edge { get; set; }
    public int Pm { get; set; }
    public HalfEdge? PriorityQueueNext { get; set; }
    public Site? Vertex { get; set; }
    public double YStar { get; set; }
    public int ReferenceCount { get; set; }
}