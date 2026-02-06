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
        
        long a2 = (long)a * a;
        long b2 = (long)b * b;

        int x = a;
        int y = 0;
        
        AddSymmetric(pixels, center, x, y);

        long d1 = 4L * a * b2 + b2 - 4L * a2;

        while (b2 * x >= a2 * (y + 1) && x <= xMax && y <= yMax)
        {
            if (d1 < 0)
            {
                d1 += 4 * (b2 * (2 * x + 2) - a2 * (2 * y + 3));
                x++;
                y++;
            }
            else
            {
                d1 -= 4 * a2 * (2 * y + 3);
                y++;
            }

            AddSymmetric(pixels, center, x, y);
        }

        long d2 = 4 * b2 * ((long)(x + 1) * (x + 1))
                 - (2L * y + 1) * (2L * y + 1) * a2
                 - 4 * a2 * b2;

        while (x <= xMax && y <= yMax)
        {
            if (d2 > 0)
            {
                d2 += 4 * (b2 * (2 * x + 3) - a2 * (2 * y + 2));
                x++;
                y++;
            }
            else
            {
                d2 += 4 * b2 * (2 * x + 3);
                x++;
            }

            AddSymmetric(pixels, center, x, y);
        }

        var result = new List<Pixel>();
        foreach (var (px, py) in pixels)
        {
            if (Math.Abs(px - center.X) <= xMax && Math.Abs(py - center.Y) <= yMax)
            {
                result.Add(new Pixel(px, py, color));
            }
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