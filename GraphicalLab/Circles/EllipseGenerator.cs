using System;
using System.Collections.Generic;

namespace GraphicalLab.Circles;

public static class EllipseGenerator
{
    public static List<Pixel> DrawEllipse(Pixel center, int a, int b, uint color = 0xFF0000FF)
    {
        var pointsSet = new HashSet<(int, int)>();

        void AddSym(int cx, int cy)
        {
            pointsSet.Add((cx, cy));
            pointsSet.Add((-cx, cy));
            pointsSet.Add((cx, -cy));
            pointsSet.Add((-cx, -cy));
        }

        int x = 0;
        int y = b;
        double a2 = (double)a * a;
        double b2 = (double)b * b;

        double d1 = b2 - a2 * b + 0.25 * a2;
        double dx = 2.0 * b2 * x;
        double dy = 2.0 * a2 * y;

        while (dx < dy)
        {
            AddSym(x, y);

            if (d1 < 0)
            {
                x++;
                dx += 2.0 * b2;
                d1 += dx + b2;
            }
            else
            {
                x++;
                y--;
                dx += 2.0 * b2;
                dy -= 2.0 * a2;
                d1 += dx - dy + b2;
            }
        }

        double d2 = b2 * (x + 0.5) * (x + 0.5) + a2 * (y - 1) * (y - 1) - a2 * b2;

        while (y >= 0)
        {
            AddSym(x, y);

            if (d2 > 0)
            {
                y--;
                dy -= 2.0 * a2;
                d2 += a2 - dy;
            }
            else
            {
                x++;
                y--;
                dx += 2.0 * b2;
                dy -= 2.0 * a2;
                d2 += dx - dy + a2;
            }
        }

        var result = new List<Pixel>(pointsSet.Count);
        foreach (var (px, py) in pointsSet)
        {
            result.Add(new Pixel(px + center.X, py + center.Y));
        }

        return result;
    }
}