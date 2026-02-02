using System;
using System.Collections.Generic;

namespace GraphicalLab.Circles;

public static class EllipseGenerator
{
    public static List<Pixel> DrawEllipse(Pixel center, int a, int b, uint color = 0xFF0000FF)
    {
        List<Pixel> pixels = [];
        if (a == 0 || b == 0) return pixels;
        int x = 0, y = b;
        
        long a2 = a * a;
        long b2 = b * b;
        long twoA2 = 2 * a2;
        long twoB2 = 2 * b2;
        
        long d1 = b2 - a2 * b + a2 / 4;
        
        while (twoB2 * x <= twoA2 * y)
        {
            pixels.Add(new Pixel(x, y));
        
            if (d1 < 0)
            {
                d1 += twoB2 * x + b2;
            }
            else
            {
                y--;
                d1 += twoB2 * x - twoA2 * y + b2;
            }
            x++;
        }
        
        long d2 = (long)(b2 * (x + 0.5) * (x + 0.5) + 
            a2 * (y - 1) * (y - 1) - a2 * b2);
    
        while (y >= 0)
        {
            pixels.Add(new Pixel(x, y));
        
            if (d2 > 0)
            {
                d2 += a2 - twoA2 * y;
            }
            else
            {
                x++;
                d2 += twoB2 * x + a2 - twoA2 * y;
            }
            y--;
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