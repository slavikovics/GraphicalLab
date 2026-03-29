using System.Collections.Generic;
using System.Linq;

namespace GraphicalLab.Voronoi;

internal class ListSiteProvider : ISiteProvider
{
    private readonly List<Site> _sites;
    private int _index;

    public ListSiteProvider(List<Site> sites)
    {
        _sites = sites;
        _index = 0;
        ClearSites();
    }

    private void ClearSites()
    {
        List<Site> uniqueSites = new List<Site>();
        var comparer = new SiteEqualityComparer();
        foreach (var site in _sites)
        {
            if (!uniqueSites.Contains(site, comparer))
            {
                uniqueSites.Add(site);
            }
        }

        _sites.Clear();
        _sites.AddRange(uniqueSites);
    }

    public Site? GetNextSite() => _index < _sites.Count ? _sites[_index++] : null;
    public int SiteCount => _sites.Count;
    public BoundingBox? GetBoundingBox() => null;
}