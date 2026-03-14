using System;
using System.Collections.Generic;
using GraphicalLab.Models;

namespace GraphicalLab.Poly;

public static class IntersectionCalculator
{
    public struct IntersectionResult
    {
        public bool HasIntersection { get; set; }
        public Pixel Point { get; set; }
        public double Parameter { get; set; }
        public int EdgeIndex { get; set; }
        public bool IsVertex { get; set; }

        public override string ToString()
        {
            if (!HasIntersection)
                return "Пересечения нет";
            string vertexInfo = IsVertex ? " (вершина)" : "";
            return $"Пересечение с ребром {EdgeIndex}{vertexInfo} в точке ({Point.X}, {Point.Y}), t={Parameter:F3}";
        }
    }

    public static IntersectionResult FindIntersection(
        Pixel segmentStart,
        Pixel segmentEnd,
        Pixel edgeStart,
        Pixel edgeEnd,
        Vector normal,
        int edgeIndex = -1,
        double epsilon = 1e-10)
    {
        Vector s = new Vector(
            segmentEnd.X - segmentStart.X,
            segmentEnd.Y - segmentStart.Y);

        Vector e = new Vector(
            edgeEnd.X - edgeStart.X,
            edgeEnd.Y - edgeStart.Y);
        
        if (Math.Abs(s.X) < epsilon && Math.Abs(s.Y) < epsilon)
        {
            return new IntersectionResult { HasIntersection = false };
        }

        double denominator = normal.X * s.X + normal.Y * s.Y;

        if (Math.Abs(denominator) < epsilon)
        {
            return new IntersectionResult { HasIntersection = false };
        }

        Vector r0MinusR1 = new Vector(
            edgeStart.X - segmentStart.X,
            edgeStart.Y - segmentStart.Y);

        double numerator = normal.X * r0MinusR1.X + normal.Y * r0MinusR1.Y;
        double t = numerator / denominator;

        if (t < -epsilon || t > 1 + epsilon)
        {
            return new IntersectionResult { HasIntersection = false };
        }
        t = Math.Max(0, Math.Min(1, t));

        double intersectX = segmentStart.X + t * s.X;
        double intersectY = segmentStart.Y + t * s.Y;
        
        double u;
        if (Math.Abs(e.X) > Math.Abs(e.Y))
        {
            if (Math.Abs(e.X) < epsilon)
                return new IntersectionResult { HasIntersection = false };
            u = (intersectX - edgeStart.X) / e.X;
        }
        else
        {
            if (Math.Abs(e.Y) < epsilon)
                return new IntersectionResult { HasIntersection = false };
            u = (intersectY - edgeStart.Y) / e.Y;
        }

        if (u >= -epsilon && u <= 1 + epsilon)
        {
            u = Math.Max(0, Math.Min(1, u));
            int x = (int)(edgeStart.X + u * e.X);
            int y = (int)(edgeStart.Y + u * e.Y);
            
            bool isVertex = (u < epsilon || u > 1 - epsilon);

            return new IntersectionResult
            {
                HasIntersection = true,
                Point = new Pixel(x, y),
                Parameter = t,
                EdgeIndex = edgeIndex,
                IsVertex = isVertex
            };
        }

        return new IntersectionResult { HasIntersection = false };
    }

    public static List<IntersectionResult> FindAllIntersections(
        Pixel segmentStart,
        Pixel segmentEnd,
        List<Pixel> polygonVertices)
    {
        var innerNormals = NormVectors.GetVectors(polygonVertices);
        var intersections = new List<IntersectionResult>();

        if (polygonVertices.Count < 3)
        {
            return intersections;
        }

        for (int i = 0; i < polygonVertices.Count; i++)
        {
            Pixel edgeStart = polygonVertices[i];
            Pixel edgeEnd = polygonVertices[(i + 1) % polygonVertices.Count];
            Vector normal = innerNormals[i];

            var intersection = FindIntersection(
                segmentStart,
                segmentEnd,
                edgeStart,
                edgeEnd,
                normal,
                i);

            if (intersection.HasIntersection)
            {
                intersections.Add(intersection);
            }
        }

        intersections = RemoveDuplicates(intersections);
        intersections.Sort((a, b) => a.Parameter.CompareTo(b.Parameter));

        return intersections;
    }
    
    private static List<IntersectionResult> RemoveDuplicates(List<IntersectionResult> intersections)
    {
        var result = new List<IntersectionResult>();
        var seen = new HashSet<string>();

        foreach (var intersection in intersections)
        {
            string key = $"{intersection.Point.X}_{intersection.Point.Y}";
            if (!seen.Contains(key))
            {
                seen.Add(key);
                result.Add(intersection);
            }
        }

        return result;
    }
}