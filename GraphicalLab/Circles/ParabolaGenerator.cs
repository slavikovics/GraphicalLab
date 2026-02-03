using System;
using System.Collections.Generic;

namespace GraphicalLab.Circles;

public static class ParabolaGenerator
{
    public static List<Pixel> DrawParabola(
        Pixel center,
        int a,
        int b,
        int yMax,
        uint color = 0xFF0000FF)
    {
        var pixels = new List<Pixel>();

        int y = 0;
        int x = 0;

        int dx = a + b;
        int ddx = 2 * a;

        int prevX = x;

        while (y <= yMax)
        {
            int from = Math.Min(prevX, x);
            int to = Math.Max(prevX, x);

            for (int xx = from; xx <= to; xx++)
            {
                pixels.Add(new Pixel(center.X + xx, center.Y + y));
                pixels.Add(new Pixel(center.X + xx, center.Y - y));
            }

            prevX = x;

            y++;
            x += dx;
            dx += ddx;
        }

        return pixels;
    }
}