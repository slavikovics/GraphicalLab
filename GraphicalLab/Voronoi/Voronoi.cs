using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GraphicalLab.Lines;
using GraphicalLab.Models;
using GraphicalLab.Triangulation;

namespace GraphicalLab.Voronoi
{
    public struct Point
    {
        public double X { get; }
        public double Y { get; }

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    public class Site
    {
        public Point Coordinate { get; }
        public int Index { get; set; }
        public int ReferenceCount { get; set; }
        public Site(Point coordinate) => Coordinate = coordinate;
    }

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

    public class Edge
    {
        public double A { get; set; }
        public double B { get; set; }
        public double C { get; set; }
        public Site?[] Region { get; } = new Site?[2];
        public Site?[] Endpoint { get; } = new Site?[2];
        public int Index { get; set; }
    }

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

    public class BoundingBox
    {
        public double XMin { get; set; }
        public double XMax { get; set; }
        public double YMin { get; set; }
        public double YMax { get; set; }
    }
    
    public interface IVoronoiOutput
    {
        void OnBisectorCreated(Edge edge);
        void OnEdgeCompleted(Edge edge);
        void OnVertexCreated(Site vertex);
        void OnSiteProcessed(Site site);
        void OnCircleEvent(Site left, Site right, Site bottom);
    }

    public interface ISiteProvider
    {
        Site? GetNextSite();
        int SiteCount { get; }
        BoundingBox? GetBoundingBox();
    }
    
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
    
    public class EdgeList
    {
        private readonly HalfEdge _leftEnd;
        private readonly HalfEdge _rightEnd;
        private readonly HalfEdge?[] _hashTable;
        private readonly int _hashSize;
        private readonly Site _bottomSite;
        private readonly double _xMin;
        private readonly double _deltaX;

        public EdgeList(Site bottomSite, BoundingBox boundingBox, int sqrtSiteCount)
        {
            _bottomSite = bottomSite;
            _xMin = boundingBox.XMin;
            _deltaX = boundingBox.XMax - boundingBox.XMin;
            _hashSize = 2 * sqrtSiteCount;
            _hashTable = new HalfEdge?[_hashSize];

            _leftEnd = CreateHalfEdge(null, 0);
            _rightEnd = CreateHalfEdge(null, 0);
            _leftEnd.Left = null;
            _leftEnd.Right = _rightEnd;
            _rightEnd.Left = _leftEnd;
            _rightEnd.Right = null;

            _hashTable[0] = _leftEnd;
            _hashTable[_hashSize - 1] = _rightEnd;
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

        private HalfEdge? GetFromHash(int bucket)
        {
            if (bucket < 0 || bucket >= _hashSize) return null;
            HalfEdge? he = _hashTable[bucket];
            if (he == null || he.Edge != null) return he;

            Log($"Hash bucket {bucket} had deleted half-edge, removing");
            _hashTable[bucket] = null;

            return null;
        }

        public HalfEdge LeftBound(Point point)
        {
            int bucket = (int)((point.X - _xMin) / _deltaX * _hashSize);
            bucket = Math.Clamp(bucket, 0, _hashSize - 1);
            Log($"LeftBound for ({point.X:F2},{point.Y:F2}), bucket={bucket}");

            HalfEdge? he = GetFromHash(bucket);
            if (he == null)
            {
                int i = 1;
                while (true)
                {
                    he = GetFromHash(bucket - i);
                    if (he != null)
                    {
                        Log($"Found at bucket {bucket - i} after {i} steps");
                        break;
                    }

                    he = GetFromHash(bucket + i);
                    if (he != null)
                    {
                        Log($"Found at bucket {bucket + i} after {i} steps");
                        break;
                    }

                    i++;
                    if (i > _hashSize)
                    {
                        Log($"Hash search failed. Dumping hash table:");
                        for (int j = 0; j < _hashSize; j++)
                        {
                            if (_hashTable[j] != null)
                                Log(
                                    $"  {j}: edge={(_hashTable[j].Edge?.Index ?? -1)}, refcnt={_hashTable[j].ReferenceCount}");
                            else
                                Log($"  {j}: null");
                        }

                        Log("Falling back to linear search.");
                        he = _leftEnd.Right;
                        while (he != _rightEnd && !GeometryHelper.RightOf(he, point))
                            he = he.Right;
                        if (he == _rightEnd)
                            he = _leftEnd;
                        else
                            he = he.Left;
                        Log("Linear search completed.");
                        break;
                    }
                }
            }

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

            if (bucket > 0 && bucket < _hashSize - 1)
            {
                if (_hashTable[bucket] != null) _hashTable[bucket]!.ReferenceCount--;
                _hashTable[bucket] = he;
                _hashTable[bucket]!.ReferenceCount++;
            }

            return he!;
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

        private static void Log(string message) => Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
    }
    
    public class EventQueue
    {
        private readonly double _yMin;
        private readonly double _yMax;
        private readonly double _deltaY;
        private readonly int _hashSize;
        private readonly HalfEdge[] _hashTable;
        private int _minBucket;
        private int _count;

        public EventQueue(BoundingBox boundingBox, int sqrtSiteCount)
        {
            _yMin = boundingBox.YMin;
            _yMax = boundingBox.YMax;
            _deltaY = boundingBox.YMax - boundingBox.YMin;
            _hashSize = 4 * sqrtSiteCount;
            _hashTable = new HalfEdge[_hashSize];
            for (int i = 0; i < _hashSize; i++) _hashTable[i] = new HalfEdge();
            _minBucket = 0;
            _count = 0;
        }

        private int GetBucket(HalfEdge halfEdge)
        {
            int bucket;
            if (halfEdge.YStar < _yMin) bucket = 0;
            else if (halfEdge.YStar >= _yMax) bucket = _hashSize - 1;
            else bucket = (int)((halfEdge.YStar - _yMin) / _deltaY * _hashSize);
            bucket = Math.Clamp(bucket, 0, _hashSize - 1);
            if (bucket < _minBucket) _minBucket = bucket;
            return bucket;
        }

        public void Insert(HalfEdge halfEdge, Site vertex, double offset)
        {
            halfEdge.Vertex = vertex;
            halfEdge.YStar = vertex.Coordinate.Y + offset;

            int bucket = GetBucket(halfEdge);
            HalfEdge last = _hashTable[bucket];
            HalfEdge? next = last.PriorityQueueNext;

            while (next != null &&
                   (halfEdge.YStar > next.YStar ||
                    (Math.Abs(halfEdge.YStar - next.YStar) < 1e-10 && vertex.Coordinate.X > next.Vertex!.Coordinate.X)))
            {
                last = next;
                next = next.PriorityQueueNext;
            }

            halfEdge.PriorityQueueNext = last.PriorityQueueNext;
            last.PriorityQueueNext = halfEdge;
            _count++;
            Log($"Inserted event at y={halfEdge.YStar:F2} (bucket {bucket})");
        }

        public void Delete(HalfEdge halfEdge)
        {
            if (halfEdge.Vertex != null)
            {
                int bucket = GetBucket(halfEdge);
                HalfEdge last = _hashTable[bucket];
                while (last.PriorityQueueNext != halfEdge) last = last.PriorityQueueNext!;
                last.PriorityQueueNext = halfEdge.PriorityQueueNext;
                _count--;
                halfEdge.Vertex = null;
                Log($"Deleted event at bucket {bucket}");
            }
        }

        public bool IsEmpty => _count == 0;

        public Point GetMinPoint()
        {
            while (_hashTable[_minBucket].PriorityQueueNext == null) _minBucket++;
            HalfEdge he = _hashTable[_minBucket].PriorityQueueNext!;
            return new Point(he.Vertex!.Coordinate.X, he.YStar);
        }

        public HalfEdge ExtractMin()
        {
            HalfEdge curr = _hashTable[_minBucket].PriorityQueueNext!;
            _hashTable[_minBucket].PriorityQueueNext = curr.PriorityQueueNext;
            _count--;
            Log($"Extracted min event at y={curr.YStar:F2}");
            return curr;
        }

        private static void Log(string message) => Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
    }
    
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

        private static void Log(string message) => Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
    }
    
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
    
    public class VoronoiResult : IDrawable
    {
        public List<Pixel> Draw(List<Pixel> points)
        {
            var sites = points.Select((p, idx) => new Site(new Point(p.X, p.Y)) { Index = idx }).ToList();
            sites.Sort((a, b) =>
            {
                int yComp = a.Coordinate.Y.CompareTo(b.Coordinate.Y);
                return yComp != 0 ? yComp : a.Coordinate.X.CompareTo(b.Coordinate.X);
            });

            var boundingBox = new BoundingBox { XMin = 0, XMax = 300, YMin = 0, YMax = 300 };

            var provider = new ListSiteProvider(sites);
            var collector = new EdgeCollector();

            var algorithm = new FortuneVoronoiAlgorithm(provider, collector, boundingBox);
            algorithm.Compute();

            var allEdges = new List<Edge>(collector.CompletedEdges);
            allEdges.AddRange(algorithm.GetAllEdges());

            var resultPixels = new List<Pixel>();
            foreach (var edge in allEdges)
            {
                var segment = ClipEdge(edge, boundingBox, sites);
                if (segment != null)
                {
                    resultPixels.AddRange(BrezenhemLineGenerator.DrawLine(segment.Value.Start, segment.Value.End));
                }
            }

            return resultPixels;
        }
        
        private (Pixel Start, Pixel End)? ClipEdge(Edge edge, BoundingBox box, List<Site> allSites)
        {
            const double eps = 1e-9;
            const double testDistance = 8.0;

            Pixel ToPixel(Site s) =>
                new Pixel((int)Math.Round(s.Coordinate.X), (int)Math.Round(s.Coordinate.Y));

            double Dist2(Pixel p, Site s)
            {
                double dx = p.X - s.Coordinate.X;
                double dy = p.Y - s.Coordinate.Y;
                return dx * dx + dy * dy;
            }

            bool BelongsToCell(Pixel p, Site owner)
            {
                double dOwner = Dist2(p, owner);

                foreach (var s in allSites)
                {
                    if (ReferenceEquals(s, owner)) continue;
                    if (Dist2(p, s) < dOwner - eps)
                        return false;
                }

                return true;
            }
            
            var s1 = edge.Region[0];
            var s2 = edge.Region[1];
            if (s1 == null || s2 == null)
                return null;

            double dx = s2.Coordinate.X - s1.Coordinate.X;
            double dy = s2.Coordinate.Y - s1.Coordinate.Y;

            double dirX = -dy;
            double dirY = dx;

            double len = Math.Sqrt(dirX * dirX + dirY * dirY);
            if (len < eps) return null;

            dirX /= len;
            dirY /= len;

            Pixel origin;
            if (edge.Endpoint[0] != null)
                origin = ToPixel(edge.Endpoint[0]!);
            else if (edge.Endpoint[1] != null)
                origin = ToPixel(edge.Endpoint[1]!);
            else
            {
                double mx = (s1.Coordinate.X + s2.Coordinate.X) / 2;
                double my = (s1.Coordinate.Y + s2.Coordinate.Y) / 2;
                origin = new Pixel((int)Math.Round(mx), (int)Math.Round(my));
            }

            if (edge.Endpoint[0] != null && edge.Endpoint[1] != null)
            {
                return ClipSegment(ToPixel(edge.Endpoint[0]!), ToPixel(edge.Endpoint[1]!), box);
            }

            if (edge.Endpoint[0] != null || edge.Endpoint[1] != null)
            {
                var known = origin;

                if (known.X < box.XMin - 1 || known.X > box.XMax + 1 ||
                    known.Y < box.YMin - 1 || known.Y > box.YMax + 1)
                    return null;

                double tx1 = known.X + testDistance * dirX;
                double ty1 = known.Y + testDistance * dirY;
                double tx2 = known.X - testDistance * dirX;
                double ty2 = known.Y - testDistance * dirY;

                var testPixel1 = new Pixel((int)Math.Round(tx1), (int)Math.Round(ty1));
                var testPixel2 = new Pixel((int)Math.Round(tx2), (int)Math.Round(ty2));

                bool ok1 = BelongsToCell(testPixel1, s1) || BelongsToCell(testPixel1, s2);
                bool ok2 = BelongsToCell(testPixel2, s1) || BelongsToCell(testPixel2, s2);

                double chosenDirX, chosenDirY;
                if (ok1 && !ok2)
                {
                    chosenDirX = dirX;
                    chosenDirY = dirY;
                }
                else if (ok2 && !ok1)
                {
                    chosenDirX = -dirX;
                    chosenDirY = -dirY;
                }
                else
                {
                    double d1 = (tx1 - known.X) * (tx1 - known.X) + (ty1 - known.Y) * (ty1 - known.Y);
                    double d2 = (tx2 - known.X) * (tx2 - known.X) + (ty2 - known.Y) * (ty2 - known.Y);
                    if (d1 > d2)
                    {
                        chosenDirX = dirX;
                        chosenDirY = dirY;
                    }
                    else
                    {
                        chosenDirX = -dirX;
                        chosenDirY = -dirY;
                    }
                }

                var intersections = new List<(double t, Pixel p)>();

                void TryAddRay(double t)
                {
                    if (t < -eps) return;

                    double x = known.X + t * chosenDirX;
                    double y = known.Y + t * chosenDirY;

                    if (x >= box.XMin - eps && x <= box.XMax + eps &&
                        y >= box.YMin - eps && y <= box.YMax + eps)
                    {
                        var pixel = new Pixel((int)Math.Round(x), (int)Math.Round(y));
                        intersections.Add((t, pixel));
                    }
                }

                if (Math.Abs(chosenDirX) > eps)
                {
                    TryAddRay((box.XMin - known.X) / chosenDirX);
                    TryAddRay((box.XMax - known.X) / chosenDirX);
                }

                if (Math.Abs(chosenDirY) > eps)
                {
                    TryAddRay((box.YMin - known.Y) / chosenDirY);
                    TryAddRay((box.YMax - known.Y) / chosenDirY);
                }

                if (intersections.Count == 0)
                    return null;

                var farthest = intersections.OrderByDescending(item => item.t).First();
                return ClipSegment(known, farthest.p, box);
            }

            double vx = origin.X;
            double vy = origin.Y;

            var fullIntersections = new List<Pixel>();

            void TryAddFull(double t)
            {
                double x = vx + t * dirX;
                double y = vy + t * dirY;
                if (x >= box.XMin - eps && x <= box.XMax + eps &&
                    y >= box.YMin - eps && y <= box.YMax + eps)
                {
                    fullIntersections.Add(new Pixel((int)Math.Round(x), (int)Math.Round(y)));
                }
            }

            if (Math.Abs(dirX) > eps)
            {
                TryAddFull((box.XMin - vx) / dirX);
                TryAddFull((box.XMax - vx) / dirX);
            }

            if (Math.Abs(dirY) > eps)
            {
                TryAddFull((box.YMin - vy) / dirY);
                TryAddFull((box.YMax - vy) / dirY);
            }

            fullIntersections = fullIntersections.Distinct().ToList();
            if (fullIntersections.Count < 2) return null;

            return (fullIntersections[0], fullIntersections[1]);
        }

        private (Pixel, Pixel)? ClipSegment(Pixel p1, Pixel p2, BoundingBox box)
        {
            int code1 = ComputeOutCode(p1, box);
            int code2 = ComputeOutCode(p2, box);
            double x1 = p1.X, y1 = p1.Y;
            double x2 = p2.X, y2 = p2.Y;

            while (true)
            {
                if ((code1 | code2) == 0)
                {
                    return (new Pixel((int)Math.Round(x1), (int)Math.Round(y1)),
                        new Pixel((int)Math.Round(x2), (int)Math.Round(y2)));
                }

                if ((code1 & code2) != 0)
                {
                    return null;
                }

                int codeOut = code1 != 0 ? code1 : code2;
                double x, y;

                if ((codeOut & 8) != 0)
                {
                    x = x1 + (x2 - x1) * (box.YMax - y1) / (y2 - y1);
                    y = box.YMax;
                }
                else if ((codeOut & 4) != 0)
                {
                    x = x1 + (x2 - x1) * (box.YMin - y1) / (y2 - y1);
                    y = box.YMin;
                }
                else if ((codeOut & 2) != 0)
                {
                    y = y1 + (y2 - y1) * (box.XMax - x1) / (x2 - x1);
                    x = box.XMax;
                }
                else
                {
                    y = y1 + (y2 - y1) * (box.XMin - x1) / (x2 - x1);
                    x = box.XMin;
                }

                if (codeOut == code1)
                {
                    x1 = x;
                    y1 = y;
                    code1 = ComputeOutCode(new Pixel((int)Math.Round(x), (int)Math.Round(y)), box);
                }
                else
                {
                    x2 = x;
                    y2 = y;
                    code2 = ComputeOutCode(new Pixel((int)Math.Round(x), (int)Math.Round(y)), box);
                }
            }
        }

        private int ComputeOutCode(Pixel p, BoundingBox box)
        {
            int code = 0;
            if (p.X < box.XMin) code |= 1;
            else if (p.X > box.XMax) code |= 2;
            if (p.Y < box.YMin) code |= 4;
            else if (p.Y > box.YMax) code |= 8;
            return code;
        }
    }
}