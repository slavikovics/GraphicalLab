using System;
using System.Collections.Generic;
using GraphicalLab.Models;

namespace GraphicalLab.Lines;

public static class BrezenhemLineGenerator
{
    public static List<Pixel> DrawLine(Pixel start, Pixel end, uint color = 0xFF0000FF)
    {
        List<Pixel> newPoints = [];
        int x1 = start.X, y1 = start.Y;
        int x2 = end.X, y2 = end.Y;

        int x = x1, y = y1;
        int dx = Math.Abs(x2 - x1);
        int dy = Math.Abs(y2 - y1);

        if (dx < dy)
        {
            var reversed = DrawLine(start.Swapped(), end.Swapped(), color);
            foreach (var point in reversed)
            {
                point.Swap();
                newPoints.Add(point);
            }

            return newPoints;
        }

        double e = (double)dy / dx - 0.5;
        newPoints.Add(new Pixel(start.X, start.Y, color));

        int sx = (x1 < x2) ? 1 : -1;
        int sy = (y1 < y2) ? 1 : -1;

        for (int i = 1; i <= dx; i++)
        {
            if (e >= 0)
            {
                y += sy;
                e -= 1;
            }

            x += sx;
            e += (double)dy / dx;
            newPoints.Add(new Pixel(x, y, color));
        }

        return newPoints;
    }
}