using System.Collections.Generic;
using Avalonia;
using GraphicalLab.Models;

namespace GraphicalLab.Poly;

public class NormVectors
{
    public static List<Pixel> BuildVectors(List<Pixel> pixels)
    {
        List<Pixel> result = [];
        if (pixels.Count < 3) return result;
        
        for (int i = 0; i < pixels.Count; i++)
        {
            var first = pixels[i];
            var second = pixels[(i + 1) % pixels.Count];
            var third = pixels[(i + 2) % pixels.Count];
            var vector = FindVector(first, second, third);
            result.Add(vector);
        }
        
        return result;
    }

    public static Pixel FindVector(Pixel first, Pixel second, Pixel third)
    {
        var v3 = new Pixel(-1 * (second.Y - first.Y), second.X - first.X);
        var prodRes = -1 * (second.Y - first.Y) * (third.X - first.X) + (second.X - first.X) * (third.Y - first.Y);

        if (prodRes > 0) v3.Invert();
        return v3;
    }
}