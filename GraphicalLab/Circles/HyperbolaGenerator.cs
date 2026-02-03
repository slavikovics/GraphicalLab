using System;
using System.Collections.Generic;

namespace GraphicalLab.Circles;

public static class HyperbolaGenerator
{
    public static List<Pixel> DrawHyperbola(
        Pixel center,
        int a,
        int b,
        int limit,
        uint color = 0xFF0000FF)
    {
        var pixels = new HashSet<(int, int)>();

        void AddSym(int x, int y)
        {
            if (Math.Abs(x) > limit || Math.Abs(y) > limit) return;

            pixels.Add((x, y));
            pixels.Add((-x, y));
            pixels.Add((x, -y));
            pixels.Add((-x, -y));
        }

        int x = a;
        int y = 0;

        int a2 = a * a;
        int b2 = b * b;

        while (Math.Abs(x) <= limit && Math.Abs(y) <= limit && b2 * x < a2 * y + a2 * limit)
        {
            AddSym(x, y);

            double d = b2 * (x + 1) * (x + 1) - a2 * (y - 0.5) * (y - 0.5) - a2 * b2;

            if (d < 0)
            {
                x++;
            }
            else
            {
                x++;
                y++;
            }
        }

        while (Math.Abs(x) <= limit && Math.Abs(y) <= limit)
        {
            AddSym(x, y);
            double d = b2 * (x + 0.5) * (x + 0.5) - a2 * (y - 1) * (y - 1) - a2 * b2;

            if (d > 0)
            {
                y++;
            }
            else
            {
                x++;
                y++;
            }
        }

        var result = new List<Pixel>(pixels.Count);
        foreach (var (px, py) in pixels)
            result.Add(new Pixel(px + center.X, py + center.Y));

        return result;
    }
}