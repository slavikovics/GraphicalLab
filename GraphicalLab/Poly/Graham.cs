using System;
using System.Collections.Generic;
using System.Linq;
using GraphicalLab.Models;

namespace GraphicalLab.Poly;

public class Graham
{
    public static List<Pixel> Draw(List<Pixel> points)
    {
        if (points.Count < 3) return [];

        Pixel start = points.OrderBy(p => p.Y).ThenBy(p => p.X).First();

        var sorted = points
            .Where(p => p != start)
            .OrderBy(p => Math.Atan2(p.Y - start.Y, p.X - start.X))
            .ThenBy(p => Math.Pow(p.X - start.X, 2) + Math.Pow(p.Y - start.Y, 2))
            .ToList();

        Stack<Pixel> hull = new Stack<Pixel>();
        hull.Push(start);
        hull.Push(sorted[0]);

        for (int i = 1; i < sorted.Count; i++)
        {
            Pixel current = sorted[i];

            while (hull.Count >= 2)
            {
                Pixel top = hull.Pop();
                Pixel second = hull.Peek();

                int cross = (top.X - second.X) * (current.Y - second.Y) -
                            (top.Y - second.Y) * (current.X - second.X);

                if (cross > 0)
                {
                    hull.Push(top);
                    break;
                }
            }

            hull.Push(current);
        }

        return hull.ToList();
    }
}