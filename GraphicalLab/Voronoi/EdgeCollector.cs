using System.Collections.Generic;

namespace GraphicalLab.Voronoi;

internal class EdgeCollector : IVoronoiOutput
{
    public List<Edge> CompletedEdges { get; } = new();

    public void OnBisectorCreated(Edge edge)
    {
    }

    public void OnEdgeCompleted(Edge edge) => CompletedEdges.Add(edge);

    public void OnVertexCreated(Site vertex)
    {
    }

    public void OnSiteProcessed(Site site)
    {
    }

    public void OnCircleEvent(Site left, Site right, Site bottom)
    {
    }
}