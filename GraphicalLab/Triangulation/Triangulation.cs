using System;
using System.Collections.Generic;
using System.Linq;
using GraphicalLab.Models;
using GraphicalLab.Poly;

namespace GraphicalLab.Triangulation;

public class Triangulation
{
    public static List<Triangle> Triangulate(List<Pixel> points)
    {
        if (points.Count < 3) return [];

        var triangles = new List<Triangle>();
        var liveEdges = new HashSet<Edge>();

        var hull = Jarvis.Draw(points);
        if (hull.Count < 3) return triangles;

        for (int i = 0; i < hull.Count; i++)
        {
            var edge = new Edge(hull[i], hull[(i + 1) % hull.Count]);
            liveEdges.Add(edge);
        }

        while (liveEdges.Count > 0)
        {
            Edge currentEdge = liveEdges.First();
            liveEdges.Remove(currentEdge);

            Pixel? conjugatePoint = FindConjugatePoint(currentEdge, points, triangles);

            if (conjugatePoint != null)
            {
                var newTriangle = new Triangle(currentEdge.A, currentEdge.B, conjugatePoint);
                triangles.Add(newTriangle);

                var edge1 = new Edge(currentEdge.A, conjugatePoint);
                var edge2 = new Edge(currentEdge.B, conjugatePoint);

                UpdateLiveEdges(liveEdges, edge1);
                UpdateLiveEdges(liveEdges, edge2);
            }
        }

        return triangles;
    }

    public static List<Pixel> DrawTriangulation(List<Pixel> points)
    {
        var triangles = Triangulate(points);
        var result = new List<Pixel>();

        foreach (var triangle in triangles)
        {
            result.AddRange(triangle.Draw());
        }

        return result;
    }

    private static Pixel? FindConjugatePoint(Edge edge, List<Pixel> points, List<Triangle> triangles)
    {
        Triangle? adjacentTriangle = FindAdjacentTriangle(edge, triangles);
        double r = double.NegativeInfinity;

        if (adjacentTriangle != null)
        {
            r = ComputeCircleParameter(edge, adjacentTriangle);
        }

        Pixel? bestPoint = null;
        double bestU = double.PositiveInfinity;

        double nx = -(edge.B.Y - edge.A.Y);
        double ny = (edge.B.X - edge.A.X);
        double normLength = Math.Sqrt(nx * nx + ny * ny);
        if (normLength > 0)
        {
            nx /= normLength;
            ny /= normLength;
        }

        double mx = (edge.A.X + edge.B.X) / 2.0;
        double my = (edge.A.Y + edge.B.Y) / 2.0;
        double halfLength = Distance(edge.A, edge.B) / 2.0;

        foreach (var point in points)
        {
            if (point == edge.A || point == edge.B) continue;

            if (Orientation(edge.A, edge.B, point) <= 0) continue;

            double dx = mx - point.X;
            double dy = my - point.Y;
            double distMSq = dx * dx + dy * dy;

            double denominator = 2.0 * (nx * dx + ny * dy);
            if (Math.Abs(denominator) < 1e-10) continue;

            double u = (halfLength * halfLength - distMSq) / denominator;

            if (u > r && u < bestU)
            {
                if (IsValidDelaunayTriangle(edge.A, edge.B, point, points))
                {
                    bestU = u;
                    bestPoint = point;
                }
            }
        }

        return bestPoint;
    }

    private static Triangle? FindAdjacentTriangle(Edge edge, List<Triangle> triangles)
    {
        foreach (var triangle in triangles)
        {
            if (ContainsEdge(triangle, edge))
            {
                return triangle;
            }
        }

        return null;
    }

    private static bool ContainsEdge(Triangle triangle, Edge edge)
    {
        return (triangle.V1 == edge.A && triangle.V2 == edge.B) ||
               (triangle.V2 == edge.A && triangle.V3 == edge.B) ||
               (triangle.V3 == edge.A && triangle.V1 == edge.B) ||
               (triangle.V1 == edge.B && triangle.V2 == edge.A) ||
               (triangle.V2 == edge.B && triangle.V3 == edge.A) ||
               (triangle.V3 == edge.B && triangle.V1 == edge.A);
    }

    private static double ComputeCircleParameter(Edge edge, Triangle triangle)
    {
        Pixel thirdVertex = GetThirdVertex(triangle, edge);

        double nx = -(edge.B.Y - edge.A.Y);
        double ny = (edge.B.X - edge.A.X);
        double normLength = Math.Sqrt(nx * nx + ny * ny);
        if (normLength > 0)
        {
            nx /= normLength;
            ny /= normLength;
        }

        double mx = (edge.A.X + edge.B.X) / 2.0;
        double my = (edge.A.Y + edge.B.Y) / 2.0;
        double halfLength = Distance(edge.A, edge.B) / 2.0;

        double dx = mx - thirdVertex.X;
        double dy = my - thirdVertex.Y;
        double distMSq = dx * dx + dy * dy;

        double denominator = 2.0 * (nx * dx + ny * dy);
        if (Math.Abs(denominator) < 1e-10) return double.NegativeInfinity;

        return (halfLength * halfLength - distMSq) / denominator;
    }

    private static Pixel GetThirdVertex(Triangle triangle, Edge edge)
    {
        if (triangle.V1 != edge.A && triangle.V1 != edge.B) return triangle.V1;
        if (triangle.V2 != edge.A && triangle.V2 != edge.B) return triangle.V2;
        return triangle.V3;
    }

    private static bool IsValidDelaunayTriangle(Pixel a, Pixel b, Pixel c, List<Pixel> points)
    {
        double ax = a.X, ay = a.Y;
        double bx = b.X, by = b.Y;
        double cx = c.X, cy = c.Y;

        double d = 2 * (ax * (by - cy) + bx * (cy - ay) + cx * (ay - by));
        if (Math.Abs(d) < 1e-10) return true;

        double ux = ((ax * ax + ay * ay) * (by - cy) +
                     (bx * bx + by * by) * (cy - ay) +
                     (cx * cx + cy * cy) * (ay - by)) / d;

        double uy = ((ax * ax + ay * ay) * (cx - bx) +
                     (bx * bx + by * by) * (ax - cx) +
                     (cx * cx + cy * cy) * (bx - ax)) / d;

        double radiusSq = (ux - ax) * (ux - ax) + (uy - ay) * (uy - ay);

        foreach (var point in points)
        {
            if (point == a || point == b || point == c) continue;

            double dx = point.X - ux;
            double dy = point.Y - uy;
            double distSq = dx * dx + dy * dy;

            if (distSq < radiusSq - 1e-9)
            {
                return false;
            }
        }

        return true;
    }

    private static void UpdateLiveEdges(HashSet<Edge> liveEdges, Edge newEdge)
    {
        if (!liveEdges.Add(newEdge))
        {
            liveEdges.Remove(newEdge);
        }
    }

    private static int Orientation(Pixel p, Pixel q, Pixel r)
    {
        long val = (long)(q.X - p.X) * (r.Y - p.Y) -
                   (long)(q.Y - p.Y) * (r.X - p.X);
        if (val > 0) return 1;
        if (val < 0) return -1;
        return 0;
    }

    private static double Distance(Pixel p1, Pixel p2)
    {
        double dx = p2.X - p1.X;
        double dy = p2.Y - p1.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }
}