using System;
using System.Collections.Generic;
using GraphicalLab.Models;

namespace GraphicalLab.Circles;

public static class ParabolaGenerator
{
    public static List<Pixel> DrawParabola(Pixel center, int p, int maxX, uint color = 0xFF0000FF)
    {
        var pointsSet = new HashSet<(int, int)>();

        void AddSym(int x, int y)
        {
            pointsSet.Add((y, x));
            pointsSet.Add((-y, x));
        }

        int x = 0;
        int y = 0;
        AddSym(x, y);

        int d1 = 1 - 2 * p;

        while (y < 2 * p && x <= maxX)
        {
            if (d1 <= 0)
            {
                d1 += 2 * y + 3;
            }
            else 
            {
                d1 += 2 * y + 3 - 4 * p;
                x++;
            }

            y++;
            AddSym(x, y);
        }

        int d2 = (2 * y + 1) * (2 * y + 1) - 16 * p * (x + 1);

        while (x <= maxX)
        {
            if (d2 <= 0)
            {
                d2 += 8 * (y + 1) - 16 * p;
                y++;
            }
            else
            {
                d2 -= 16 * p;
            }

            x++;
            AddSym(x, y);
        }

        var result = new List<Pixel>(pointsSet.Count);
        foreach (var (px, py) in pointsSet)
        {
            result.Add(new Pixel(center.X + px, center.Y + py, color));
        }

        return result;
    }
}