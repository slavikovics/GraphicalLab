using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GraphicalLab.Voronoi;

public class EdgeList
{
    private readonly HalfEdge _leftEnd;
    private readonly HalfEdge _rightEnd;
    private readonly Site _bottomSite;

    public EdgeList(Site bottomSite, BoundingBox boundingBox, int sqrtSiteCount)
    {
        _bottomSite = bottomSite;

        _leftEnd = CreateHalfEdge(null, 0);
        _rightEnd = CreateHalfEdge(null, 0);
        _leftEnd.Left = null;
        _leftEnd.Right = _rightEnd;
        _rightEnd.Left = _leftEnd;
        _rightEnd.Right = null;
    }

    private static HalfEdge CreateHalfEdge(Edge? edge, int pm) =>
        new HalfEdge
        {
            Edge = edge,
            Pm = pm,
            PriorityQueueNext = null,
            Vertex = null,
            ReferenceCount = 0
        };

    public void Insert(HalfEdge leftBound, HalfEdge newHalfEdge)
    {
        newHalfEdge.Left = leftBound;
        newHalfEdge.Right = leftBound.Right;
        leftBound.Right!.Left = newHalfEdge;
        leftBound.Right = newHalfEdge;
        Log($"Inserted half-edge at {leftBound.GetHashCode()}");
    }

    public HalfEdge LeftBound(Point point)
    {
        HalfEdge he = _leftEnd.Right;
        int steps = 0;

        if (he == _leftEnd || (he != _rightEnd && GeometryHelper.RightOf(he, point)))
        {
            do
            {
                he = he.Right!;
                steps++;
                if (steps > 10000) throw new InvalidOperationException("Right walk exceeded limit");
            } while (he != _rightEnd && GeometryHelper.RightOf(he, point));

            he = he.Left!;
        }
        else
        {
            do
            {
                he = he.Left!;
                steps++;
                if (steps > 10000) throw new InvalidOperationException("Left walk exceeded limit");
            } while (he != _leftEnd && !GeometryHelper.RightOf(he, point));
        }

        Log($"Walk steps: {steps}");

        return he;
    }

    public void Delete(HalfEdge halfEdge)
    {
        halfEdge.Left!.Right = halfEdge.Right;
        halfEdge.Right!.Left = halfEdge.Left;
        halfEdge.Edge = null;
        Log($"Deleted half-edge {halfEdge.GetHashCode()}");
    }

    public HalfEdge Right(HalfEdge halfEdge) => halfEdge.Right!;
    public HalfEdge Left(HalfEdge halfEdge) => halfEdge.Left!;

    public Site LeftRegion(HalfEdge halfEdge)
    {
        if (halfEdge.Edge == null) return _bottomSite;
        return halfEdge.Pm == 0 ? halfEdge.Edge.Region[0]! : halfEdge.Edge.Region[1]!;
    }

    public Site RightRegion(HalfEdge halfEdge)
    {
        if (halfEdge.Edge == null) return _bottomSite;
        return halfEdge.Pm == 0 ? halfEdge.Edge.Region[1]! : halfEdge.Edge.Region[0]!;
    }

    public IEnumerable<HalfEdge> GetAllHalfEdges()
    {
        HalfEdge? current = _leftEnd.Right;
        while (current != null && current != _rightEnd)
        {
            yield return current;
            current = current.Right;
        }
    }

    private static void Log(string message) => Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
}