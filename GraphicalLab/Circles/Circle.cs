using System.Collections.Generic;

namespace GraphicalLab.Circles;

public static class Circle
{
    public static List<Pixel> DrawCircle(Pixel center, int radius, uint color = 0xFF0000FF)
    {
        List<Pixel> pixels = [];
        int x0 = center.X, y0 = center.Y;
        int x = x0, y = y0 + radius;
        double lim = 0;
        int delta = 2 - 2 * radius;
        pixels.Add(new Pixel(x, y));

        while (y > lim)
        {
            if (delta > 0)
            {
                var betaStar = 2 * delta - 2 * x - 1;
                if (betaStar > 0)
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

            if (delta < 0)
            {
                var beta = 2 * delta + 2 * y - 1;
                if (beta <= 0)
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

            if (delta == 0)
            {
                x += 1;
                y -= 1;
                delta += 2 * x - 2 * y + 2;
            }
            
            pixels.Add(new Pixel(x, y));
        }
        
        return pixels;
    }
}