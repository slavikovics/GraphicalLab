using System;
using System.Collections.Generic;
using System.Linq;
using GraphicalLab.Models;

namespace GraphicalLab.Fill;

public static class FillAlgorithms
{
    public struct Edge
    {
        public int YMax;
        public double X;
        public double Dx;
        public int YMin;

        public Edge(int yMax, double x, double dx, int yMin)
        {
            YMax = yMax;
            X = x;
            Dx = dx;
            YMin = yMin;
        }
    }

    public static List<Pixel> ScanlineWithSortedEdges(List<Pixel> polygon, uint[,] pixels, int width, int height,
        uint color)
    {
        if (polygon.Count < 3) return [];
        var result = new List<Pixel>();
        var edges = new List<Edge>();
        int yMin = polygon.Min(p => p.Y);
        int yMax = polygon.Max(p => p.Y);

        yMin = Math.Max(0, yMin);
        yMax = Math.Min(height - 1, yMax);

        for (int i = 0; i < polygon.Count; i++)
        {
            Pixel p1 = polygon[i];
            Pixel p2 = polygon[(i + 1) % polygon.Count];

            if (p1.Y == p2.Y) continue;

            if (p1.Y > p2.Y)
            {
                (p1, p2) = (p2, p1);
            }

            double dx = (double)(p2.X - p1.X) / (p2.Y - p1.Y);
            edges.Add(new Edge(p2.Y, p1.X, dx, p1.Y));
        }

        for (int y = yMin; y <= yMax; y++)
        {
            var activeEdges = edges.Where(e => e.YMin <= y && e.YMax > y)
                .Select(e => new Edge(e.YMax, e.X + e.Dx * (y - e.YMin), e.Dx, e.YMin))
                .OrderBy(e => e.X)
                .ToList();

            for (int i = 0; i < activeEdges.Count - 1; i += 2)
            {
                int xStart = (int)Math.Ceiling(activeEdges[i].X);
                int xEnd = (int)Math.Floor(activeEdges[i + 1].X);

                xStart = Math.Max(0, xStart);
                xEnd = Math.Min(width - 1, xEnd);

                for (int x = xStart; x <= xEnd; x++)
                {
                    pixels[x, y] = color;
                    result.Add(new Pixel(x, y, color));
                }
            }
        }

        return result;
    }

    public static List<Pixel> ScanlineWithAet(List<Pixel> polygon, uint[,] pixels, int width, int height, uint color)
    {
        if (polygon.Count < 3) return [];
        var result = new List<Pixel>();
        var edges = new List<Edge>();
        int yMin = polygon.Min(p => p.Y);
        int yMax = polygon.Max(p => p.Y);

        yMin = Math.Max(0, yMin);
        yMax = Math.Min(height - 1, yMax);

        for (int i = 0; i < polygon.Count; i++)
        {
            Pixel p1 = polygon[i];
            Pixel p2 = polygon[(i + 1) % polygon.Count];

            if (p1.Y == p2.Y) continue;

            if (p1.Y > p2.Y)
            {
                (p1, p2) = (p2, p1);
            }

            double dx = (double)(p2.X - p1.X) / (p2.Y - p1.Y);
            edges.Add(new Edge(p2.Y, p1.X, dx, p1.Y));
        }

        var sortedEdges = edges.OrderBy(e => e.YMin).ToList();
        var activeEdges = new List<Edge>();

        for (int y = yMin; y <= yMax; y++)
        {
            activeEdges.AddRange(sortedEdges.Where(e => e.YMin == y));
            activeEdges.RemoveAll(e => e.YMax == y);
            activeEdges = activeEdges.OrderBy(e => e.X).ToList();

            for (int i = 0; i < activeEdges.Count - 1; i += 2)
            {
                int xStart = (int)Math.Ceiling(activeEdges[i].X);
                int xEnd = (int)Math.Floor(activeEdges[i + 1].X);

                xStart = Math.Max(0, xStart);
                xEnd = Math.Min(width - 1, xEnd);

                for (int x = xStart; x <= xEnd; x++)
                {
                    pixels[x, y] = color;
                    result.Add(new Pixel(x, y, color));
                }
            }

            for (int j = 0; j < activeEdges.Count; j++)
            {
                var edge = activeEdges[j];
                edge.X += edge.Dx;
                activeEdges[j] = edge;
            }
        }

        return result;
    }

    public static List<Pixel> SimpleFloodFill(Pixel start, uint[,] pixels, int width, int height, uint fillColor)
    {
        var result = new List<Pixel>();

        if (start.X < 0 || start.X >= width || start.Y < 0 || start.Y >= height)
            return result;

        uint targetColor = pixels[start.X, start.Y];

        if (targetColor == fillColor)
            return result;

        var stack = new Stack<Pixel>();
        stack.Push(start);

        while (stack.Count > 0)
        {
            Pixel current = stack.Pop();

            if (current.X < 0 || current.X >= width || current.Y < 0 || current.Y >= height)
                continue;

            if (pixels[current.X, current.Y] == targetColor)
            {
                pixels[current.X, current.Y] = fillColor;
                result.Add(new Pixel(current.X, current.Y, fillColor));

                stack.Push(new Pixel(current.X + 1, current.Y));
                stack.Push(new Pixel(current.X - 1, current.Y));
                stack.Push(new Pixel(current.X, current.Y + 1));
                stack.Push(new Pixel(current.X, current.Y - 1));
            }
        }

        return result;
    }

    public static List<Pixel> ScanlineFloodFill(Pixel start, uint[,] pixels, int width, int height, uint fillColor)
    {
        var result = new List<Pixel>();

        if (start.X < 0 || start.X >= width || start.Y < 0 || start.Y >= height)
            return result;

        uint targetColor = pixels[start.X, start.Y];

        if (targetColor == fillColor)
            return result;

        var stack = new Stack<Pixel>();
        stack.Push(start);

        while (stack.Count > 0)
        {
            Pixel current = stack.Pop();
            int x = current.X;
            int y = current.Y;

            while (x >= 0 && pixels[x, y] == targetColor)
            {
                x--;
            }

            x++;

            bool spanAbove = false;
            bool spanBelow = false;

            while (x < width && pixels[x, y] == targetColor)
            {
                pixels[x, y] = fillColor;
                result.Add(new Pixel(x, y, fillColor));

                if (!spanAbove && y > 0 && pixels[x, y - 1] == targetColor)
                {
                    stack.Push(new Pixel(x, y - 1));
                    spanAbove = true;
                }
                else if (spanAbove && y > 0 && pixels[x, y - 1] != targetColor)
                {
                    spanAbove = false;
                }

                if (!spanBelow && y < height - 1 && pixels[x, y + 1] == targetColor)
                {
                    stack.Push(new Pixel(x, y + 1));
                    spanBelow = true;
                }
                else if (spanBelow && y < height - 1 && pixels[x, y + 1] != targetColor)
                {
                    spanBelow = false;
                }

                x++;
            }
        }

        return result;
    }
}