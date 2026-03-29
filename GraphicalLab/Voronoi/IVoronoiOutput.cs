namespace GraphicalLab.Voronoi;

public interface IVoronoiOutput
{
    void OnBisectorCreated(Edge edge);
    void OnEdgeCompleted(Edge edge);
    void OnVertexCreated(Site vertex);
    void OnSiteProcessed(Site site);
    void OnCircleEvent(Site left, Site right, Site bottom);
}