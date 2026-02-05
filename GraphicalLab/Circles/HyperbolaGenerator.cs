using System;
using System.Collections.Generic;

namespace GraphicalLab.Circles;

public static class HyperbolaGenerator
{
    public static List<Pixel> DrawHyperbola(
        Pixel center,
        int a,
        int b,
        int xMax,
        int yMax,
        uint color = 0xFF0000FF)
    {
        var pixels = new HashSet<(int, int)>();

        double a2 = (double)a * a;
        double b2 = (double)b * b;
        
        for (int x = a; x <= xMax; x++)
        {
            double t = (x * x) / a2 - 1.0;
            if (t < 0) continue;

            int y = (int)Math.Round(Math.Sqrt(b2 * t));
            AddSymmetric(pixels, center, x, y);
        }

        for (int y = 0; y <= yMax; y++)
        {
            double t = 1.0 + (y * y) / b2;

            int x = (int)Math.Round(a * Math.Sqrt(t));
            if (x < a || x > xMax) continue;

            AddSymmetric(pixels, center, x, y);
        }
        
        List<Pixel> result = new();
        foreach (var (x, y) in pixels)
        {
            if (x >= 0 && x <= xMax && y >= 0 && y <= yMax)
                result.Add(new Pixel(x, y, color));
        }

        return result;
    }

    private static void AddSymmetric(
        HashSet<(int, int)> set,
        Pixel center,
        int x,
        int y)
    {
        set.Add((center.X + x, center.Y + y));
        set.Add((center.X - x, center.Y + y));
        set.Add((center.X - x, center.Y - y));
        set.Add((center.X + x, center.Y - y));
    }
}