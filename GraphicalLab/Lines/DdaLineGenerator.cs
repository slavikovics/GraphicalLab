using System;
using System.Collections.Generic;

namespace GraphicalLab.Lines;

public static class DdaLineGenerator
{
    public static List<Pixel> DrawLine(Pixel start, Pixel end, uint color)
    {
        List<Pixel> newPoints = [];
        double dx = end.X - start.X;
        double dy = end.Y - start.Y;

        int steps = (int)Math.Max(Math.Abs(dx), Math.Abs(dy));
        if (steps == 0)
        {
            newPoints.Add(start);
            return newPoints;
        }

        double xIncrement = dx / steps;
        double yIncrement = dy / steps;

        double x = start.X;
        double y = start.Y;

        for (int i = 0; i <= steps; i++)
        {
            newPoints.Add(new Pixel(x, y, color));
            x += xIncrement;
            y += yIncrement;
        }

        return newPoints;
    }
}