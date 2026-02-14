using System.Collections.Generic;
using GraphicalLab.Models;

namespace GraphicalLab.Circles;

public static class CircleGenerator
{
    public static void MoveCoordinates(Pixel center, List<Pixel> pixels)
    {
        foreach (var pixel in pixels)
        {
            pixel.X += center.X;
            pixel.Y += center.Y;
        }
    }

    public static List<Pixel> FlipHorizontally(Pixel center, List<Pixel> pixels)
    {
        List<Pixel> flipped = [];
        foreach (var pixel in pixels)
        {
            var dy = pixel.Y - center.Y;
            flipped.Add(new Pixel(pixel.X, center.Y - dy));
        }

        return flipped;
    }

    public static List<Pixel> FlipVertically(Pixel center, List<Pixel> pixels)
    {
        List<Pixel> flipped = [];
        foreach (var pixel in pixels)
        {
            var dx = pixel.X - center.X;
            flipped.Add(new Pixel(center.X - dx, pixel.Y));
        }

        return flipped;
    }

    private static int CalculateDStar(int delta, int x)
    {
        return 2 * delta - 2 * x - 1;
    }

    private static int CalculateD(int delta, int y)
    {
        return 2 * delta + 2 * y - 1;
    }

    private static int MoveD(int x, int y)
    {
        return 2 * x - 2 * y + 2;
    }

    private static int MoveV(int y)
    {
        return 2 * y + 1;
    }

    private static int MoveH(int x)
    {
        return 2 * x + 1;
    }

    public static List<Pixel> DrawCircle(Pixel center, int radius, uint color = 0xFF0000FF)
    {
        List<Pixel> pixels = [];
        int x = 0, y = radius;
        double lim = 0;
        int delta = 2 - 2 * radius;
        pixels.Add(new Pixel(x, y));

        while (y > lim)
        {
            if (delta > 0)
            {
                var dStar = CalculateDStar(delta, x);
                if (dStar > 0)
                {
                    y -= 1;
                    delta -= MoveV(y);
                }
                else
                {
                    x += 1;
                    y -= 1;
                    delta += MoveD(x, y);
                }
            }

            else if (delta < 0)
            {
                var d = CalculateD(delta, y);
                if (d <= 0)
                {
                    x += 1;
                    delta += MoveH(x);
                }
                else
                {
                    x += 1;
                    y -= 1;
                    delta += MoveD(x, y);
                }
            }

            else
            {
                x += 1;
                y -= 1;
                delta += MoveD(x, y);
            }

            pixels.Add(new Pixel(x, y));
        }

        MoveCoordinates(center, pixels);
        var q1 = FlipHorizontally(center, pixels);
        var q2 = FlipVertically(center, pixels);
        var q3 = FlipVertically(center, q1);
        pixels.AddRange(q1);
        pixels.AddRange(q2);
        pixels.AddRange(q3);
        return pixels;
    }
}