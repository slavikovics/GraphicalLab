using System;
using System.Collections.Generic;

namespace GraphicalLab.Voronoi;

public class SiteEqualityComparer : EqualityComparer<Site>
{
    public override bool Equals(Site? x, Site? y)
    {
        double tolerance = 1e-6;
        if (x is null || y is null) return false;
        if (Math.Abs(x.Coordinate.X - y.Coordinate.X) < tolerance &&
            Math.Abs(x.Coordinate.Y - y.Coordinate.Y) < tolerance) return true;
        return false;
    }

    public override int GetHashCode(Site obj)
    {
        throw new NotImplementedException();
    }
}