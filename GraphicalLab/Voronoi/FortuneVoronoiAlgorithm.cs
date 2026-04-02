using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace GraphicalLab.Voronoi;

public class FortuneVoronoiAlgorithm
{
    private readonly ISiteProvider _siteProvider;
    private readonly IVoronoiOutput _output;
    private readonly BoundingBox _boundingBox;
    private Site? _bottomSite;
    private EdgeList? _edgeList;
    private EventQueue? _eventQueue;
    private int _edgeCount;
    private int _vertexCount;

    public FortuneVoronoiAlgorithm(ISiteProvider siteProvider, IVoronoiOutput output, BoundingBox boundingBox)
    {
        _siteProvider = siteProvider;
        _output = output;
        _boundingBox = boundingBox;
    }

    public void Compute()
    {
        int sqrtSites = (int)Math.Sqrt(_siteProvider.SiteCount + 4);
        _eventQueue = new EventQueue(_boundingBox, sqrtSites);
        _bottomSite = _siteProvider.GetNextSite();
        if (_bottomSite == null) return;

        _output.OnSiteProcessed(_bottomSite);
        Log($"Bottom site: ({_bottomSite.Coordinate.X:F2}, {_bottomSite.Coordinate.Y:F2})");
        _edgeList = new EdgeList(_bottomSite, _boundingBox, sqrtSites);

        Site? currentSite = _siteProvider.GetNextSite();
        int iteration = 0;
        int maxIterations = 100000;

        while (true)
        {
            iteration++;
            if (iteration > maxIterations)
            {
                Log("Max iterations reached – aborting.");
                break;
            }

            Point minEventPoint = default;
            bool hasEvent = !_eventQueue.IsEmpty;
            if (hasEvent) minEventPoint = _eventQueue.GetMinPoint();

            if (currentSite != null && (!hasEvent ||
                                        currentSite.Coordinate.Y < minEventPoint.Y ||
                                        (Math.Abs(currentSite.Coordinate.Y - minEventPoint.Y) < 1e-10 &&
                                         currentSite.Coordinate.X < minEventPoint.X)))
            {
                Log(
                    $"Processing site event: site {currentSite.Index} at ({currentSite.Coordinate.X:F2},{currentSite.Coordinate.Y:F2})");
                _output.OnSiteProcessed(currentSite);

                HalfEdge leftBound = _edgeList.LeftBound(currentSite.Coordinate);
                HalfEdge rightBound = _edgeList.Right(leftBound);
                Site leftSite = _edgeList.RightRegion(leftBound);
                Edge bisector = GeometryHelper.CreateBisector(leftSite, currentSite, _edgeCount++, _output);

                HalfEdge leftHalfEdge = new HalfEdge { Edge = bisector, Pm = 0 };
                _edgeList.Insert(leftBound, leftHalfEdge);

                Site? intersection = GeometryHelper.Intersect(leftBound, leftHalfEdge);
                if (intersection != null)
                {
                    _eventQueue.Delete(leftBound);
                    _eventQueue.Insert(leftBound, intersection, GeometryHelper.Distance(intersection, currentSite));
                    Log(
                        $"Inserted intersection event for leftBound at ({intersection.Coordinate.X:F2},{intersection.Coordinate.Y:F2})");
                }

                leftBound = leftHalfEdge;
                HalfEdge rightHalfEdge = new HalfEdge { Edge = bisector, Pm = 1 };
                _edgeList.Insert(leftBound, rightHalfEdge);

                intersection = GeometryHelper.Intersect(rightHalfEdge, rightBound);
                if (intersection != null)
                {
                    _eventQueue.Insert(rightHalfEdge, intersection,
                        GeometryHelper.Distance(intersection, currentSite));
                    Log(
                        $"Inserted intersection event for rightHalfEdge at ({intersection.Coordinate.X:F2},{intersection.Coordinate.Y:F2})");
                }

                currentSite = _siteProvider.GetNextSite();
            }
            else if (hasEvent)
            {
                Log("Processing circle event");
                HalfEdge leftHalfEdge = _eventQueue.ExtractMin();
                HalfEdge leftLeft = _edgeList.Left(leftHalfEdge);
                HalfEdge rightHalfEdge = _edgeList.Right(leftHalfEdge);
                HalfEdge rightRight = _edgeList.Right(rightHalfEdge);

                Site leftSite = _edgeList.LeftRegion(leftHalfEdge);
                Site rightSite = _edgeList.RightRegion(rightHalfEdge);
                Site bottomSite = _edgeList.RightRegion(leftHalfEdge);

                Log($"Circle event: left={leftSite.Index}, right={rightSite.Index}, bottom={bottomSite.Index}");
                _output.OnCircleEvent(leftSite, rightSite, bottomSite);

                Site vertex = leftHalfEdge.Vertex!;
                _output.OnVertexCreated(vertex);
                vertex.Index = _vertexCount++;
                Log($"Vertex created at ({vertex.Coordinate.X:F2},{vertex.Coordinate.Y:F2})");

                CompleteEdge(leftHalfEdge.Edge!, leftHalfEdge.Pm, vertex);
                CompleteEdge(rightHalfEdge.Edge!, rightHalfEdge.Pm, vertex);

                _edgeList.Delete(leftHalfEdge);
                _eventQueue.Delete(rightHalfEdge);
                _edgeList.Delete(rightHalfEdge);

                int pm = 0;
                if (leftSite.Coordinate.Y > rightSite.Coordinate.Y)
                {
                    (leftSite, rightSite) = (rightSite, leftSite);
                    pm = 1;
                }

                Edge newEdge = GeometryHelper.CreateBisector(leftSite, rightSite, _edgeCount++, _output);
                HalfEdge newHalfEdge = new HalfEdge { Edge = newEdge, Pm = pm };
                _edgeList.Insert(leftLeft, newHalfEdge);

                CompleteEdge(newEdge, 1 - pm, vertex);

                Site? newIntersection = GeometryHelper.Intersect(leftLeft, newHalfEdge);
                if (newIntersection != null)
                {
                    _eventQueue.Delete(leftLeft);
                    _eventQueue.Insert(leftLeft, newIntersection,
                        GeometryHelper.Distance(newIntersection, leftSite));
                    Log(
                        $"Inserted new intersection for leftLeft at ({newIntersection.Coordinate.X:F2},{newIntersection.Coordinate.Y:F2})");
                }

                newIntersection = GeometryHelper.Intersect(newHalfEdge, rightRight);
                if (newIntersection != null)
                {
                    _eventQueue.Insert(newHalfEdge, newIntersection,
                        GeometryHelper.Distance(newIntersection, leftSite));
                    Log(
                        $"Inserted new intersection for newHalfEdge at ({newIntersection.Coordinate.X:F2},{newIntersection.Coordinate.Y:F2})");
                }
            }
            else
            {
                break;
            }
        }

        if (_edgeList != null)
        {
            foreach (var halfEdge in _edgeList.GetAllHalfEdges())
            {
                if (halfEdge.Edge != null)
                {
                    _output.OnEdgeCompleted(halfEdge.Edge);
                }
            }
        }

        Log("Algorithm finished.");
    }

    private void CompleteEdge(Edge edge, int endpointIndex, Site vertex)
    {
        edge.Endpoint[endpointIndex] = vertex;
        if (edge.Endpoint[1 - endpointIndex] != null)
        {
            _output.OnEdgeCompleted(edge);
            Log($"Edge {edge.Index} completed.");
        }
    }

    public List<Edge> GetAllEdges()
    {
        var edges = new List<Edge>();
        if (_edgeList == null) return edges;
        foreach (var halfEdge in _edgeList.GetAllHalfEdges())
        {
            if (halfEdge.Edge != null && !edges.Contains(halfEdge.Edge))
                edges.Add(halfEdge.Edge);
        }

        return edges;
    }

    private static void Log(string message) => Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
}