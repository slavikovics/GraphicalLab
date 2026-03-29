using System;

namespace GraphicalLab.Voronoi;

public static class GeometryHelper
{
    public static Edge CreateBisector(Site left, Site right, int edgeIndex, IVoronoiOutput output)
    {
        double dx = right.Coordinate.X - left.Coordinate.X;
        double dy = right.Coordinate.Y - left.Coordinate.Y;
        double adx = Math.Abs(dx);
        double ady = Math.Abs(dy);

        Edge edge = new Edge
        {
            Index = edgeIndex,
            Region = { [0] = left, [1] = right },
            Endpoint = { [0] = null, [1] = null }
        };

        edge.C = left.Coordinate.X * dx + left.Coordinate.Y * dy + (dx * dx + dy * dy) * 0.5;
        if (adx > ady)
        {
            edge.A = 1.0;
            edge.B = dy / dx;
            edge.C /= dx;
        }
        else
        {
            edge.B = 1.0;
            edge.A = dx / dy;
            edge.C /= dy;
        }

        output.OnBisectorCreated(edge);
        return edge;
    }

    public static Site? Intersect(HalfEdge halfEdge1, HalfEdge halfEdge2)
    {
        Edge? e1 = halfEdge1.Edge;
        Edge? e2 = halfEdge2.Edge;
        if (e1 == null || e2 == null) return null;
        if (e1.Region[1] == e2.Region[1]) return null;

        double d = (e1.A * e2.B) - (e1.B * e2.A);
        if (Math.Abs(d) < 1e-10) return null;

        double x = (e1.C * e2.B - e2.C * e1.B) / d;
        double y = (e2.C * e1.A - e1.C * e2.A) / d;

        HalfEdge leftHalfEdge;
        Edge leftEdge;
        if (e1.Region[1]!.Coordinate.Y < e2.Region[1]!.Coordinate.Y ||
            (Math.Abs(e1.Region[1]!.Coordinate.Y - e2.Region[1]!.Coordinate.Y) < 1e-10 &&
             e1.Region[1]!.Coordinate.X < e2.Region[1]!.Coordinate.X))
        {
            leftHalfEdge = halfEdge1;
            leftEdge = e1;
        }
        else
        {
            leftHalfEdge = halfEdge2;
            leftEdge = e2;
        }

        bool rightOfSite = x >= leftEdge.Region[1]!.Coordinate.X;
        if ((rightOfSite && leftHalfEdge.Pm == 0) || (!rightOfSite && leftHalfEdge.Pm == 1))
            return null;

        return new Site(new Point(x, y));
    }

    public static bool RightOf(HalfEdge halfEdge, Point point)
    {
        Edge e = halfEdge.Edge!;
        Site topSite = e.Region[1]!;
        bool rightOfSite = point.X > topSite.Coordinate.X;

        if (rightOfSite && halfEdge.Pm == 0) return true;
        if (!rightOfSite && halfEdge.Pm == 1) return false;

        bool above;
        if (Math.Abs(e.A - 1.0) < 1e-10)
        {
            double dyp = point.Y - topSite.Coordinate.Y;
            double dxp = point.X - topSite.Coordinate.X;
            bool fast = false;

            if ((!rightOfSite && e.B < 0.0) || (rightOfSite && e.B >= 0.0))
            {
                above = dyp >= e.B * dxp;
                fast = true;
            }
            else
            {
                above = (point.X + point.Y * e.B) > e.C;
                if (e.B < 0.0) above = !above;
                if (!above) fast = true;
            }

            if (!fast)
            {
                double dxs = topSite.Coordinate.X - e.Region[0]!.Coordinate.X;
                above = (e.B * (dxp * dxp - dyp * dyp)) <
                        (dxs * dyp * (1.0 + 2.0 * dxp / dxs + e.B * e.B));
                if (e.B < 0.0) above = !above;
            }
        }
        else
        {
            double yl = e.C - e.A * point.X;
            double t1 = point.Y - yl;
            double t2 = point.X - topSite.Coordinate.X;
            double t3 = yl - topSite.Coordinate.Y;
            above = (t1 * t1) > (t2 * t2 + t3 * t3);
        }

        return halfEdge.Pm == 0 ? above : !above;
    }

    public static double Distance(Site a, Site b) =>
        Math.Sqrt(Math.Pow(a.Coordinate.X - b.Coordinate.X, 2) + Math.Pow(a.Coordinate.Y - b.Coordinate.Y, 2));
}