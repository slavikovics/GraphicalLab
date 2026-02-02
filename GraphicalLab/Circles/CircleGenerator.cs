using System.Collections.Generic;

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
                var dStar = 2 * delta - 2 * x - 1;
                if (dStar > 0)
                {
                    y -= 1;
                    delta -= 2 * y + 1;
                }
                else
                {
                    x += 1;
                    y -= 1;
                    delta += 2 * x - 2 * y + 2;
                }
            }

            else if (delta < 0)
            {
                var d = 2 * delta + 2 * y - 1;
                if (d <= 0)
                {
                    x += 1;
                    delta += 2 * x + 1;
                }
                else
                {
                    x += 1;
                    y -= 1;
                    delta += 2 * x - 2 * y + 2;
                }
            }

            else
            {
                x += 1;
                y -= 1;
                delta += 2 * x - 2 * y + 2;
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