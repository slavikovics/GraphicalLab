namespace GraphicalLab.Voronoi;

public interface ISiteProvider
{
    Site? GetNextSite();
    int SiteCount { get; }
    BoundingBox? GetBoundingBox();
}