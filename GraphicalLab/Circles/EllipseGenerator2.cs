using System.Collections.Generic;

namespace GraphicalLab.Circles;

public class EllipseGenerator2
{
    private static int CalculateDStar(int delta, int x, int b)
    {
        return 2 * (delta + b * b * x) - 1;
    }

    private static int CalculateD(int delta, int y, int a)
    {
        return 2 * (delta + a * a * y) - 1;
    }

    private static int MoveD(int x, int y, int a, int b)
    {
        return b * b * (2 * x + 1) + a * a * (1 - 2 * y);
    }

    private static int MoveV(int y, int a)
    {
        return a * a * (1 - 2 * y);
    }

    private static int MoveH(int x, int b)
    {
        return b * b * (2 * x + 1);
    }

    public static List<Pixel> DrawEllipse(Pixel center, int a, int b, uint color = 0xFF0000FF)
    {
        List<Pixel> pixels = [];
        int x = 0, y = b;
        double lim = 0;
        int delta = a * a + b * b - 2 * a * a * b;
        pixels.Add(new Pixel(x, y));

        while (b*b*x <= a*a*y)
        {
            if (delta > 0)
            {
                var dStar = CalculateDStar(delta, x, b);
                if (dStar > 0)
                {
                    y -= 1;
                    delta -= MoveV(y, a);
                }
                else
                {
                    x += 1;
                    y -= 1;
                    delta += MoveD(x, y, a, b);
                }
            }

            else if (delta < 0)
            {
                var d = CalculateD(delta, y, a);
                if (d <= 0)
                {
                    x += 1;
                    delta += MoveH(x, b);
                }
                else
                {
                    x += 1;
                    y -= 1;
                    delta += MoveD(x, y, a, b);
                }
            }

            else
            {
                x += 1;
                y -= 1;
                delta += MoveD(x, y, a, b);
            }

            pixels.Add(new Pixel(x, y));
        }

        CircleGenerator.MoveCoordinates(center, pixels);
        var q1 = CircleGenerator.FlipHorizontally(center, pixels);
        var q2 = CircleGenerator.FlipVertically(center, pixels);
        var q3 = CircleGenerator.FlipVertically(center, q1);
        pixels.AddRange(q1);
        pixels.AddRange(q2);
        pixels.AddRange(q3);
        return pixels;
    }
}