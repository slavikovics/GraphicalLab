using System.Collections.Generic;
using GraphicalLab.Lines;
using GraphicalLab.Models;

namespace GraphicalLab.Poly;

public class NormVectors
{
    public static List<Pixel> DrawNorms(List<Pixel> pixels)
    {
        List<Pixel> result = [];
        if (pixels.Count < 3) return result;

        var vectors = BuildVectors(pixels);
        for (int i = 0; i < pixels.Count; i++)
        {
            var first = pixels[i];
            var second = pixels[(i + 1) % pixels.Count];
            Pixel startPoint = new Pixel((first.X + second.X) / 2, (first.Y + second.Y) / 2);
            Pixel endPoint = new Pixel(startPoint.X + vectors[i].X, startPoint.Y + vectors[i].Y);
            result.AddRange(BrezenhemLineGenerator.DrawTrimmedLine(startPoint, endPoint, 20));
        }
        
        return result;
    }

    public static List<Vector> GetVectors(List<Pixel> pixels)
    {
        List<Vector> result = [];

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
    
    private static List<Pixel> BuildVectors(List<Pixel> pixels)
    {
        List<Pixel> result = [];

        for (int i = 0; i < pixels.Count; i++)
        {
            var first = pixels[i];
            var second = pixels[(i + 1) % pixels.Count];
            var third = pixels[(i + 2) % pixels.Count];
            var vector = FindVector(first, second, third);
            result.Add(vector.ToPixel());
        }

        return result;
    }

    private static Vector FindVector(Pixel first, Pixel second, Pixel third)
    {
        var v3 = new Vector(-1 * (second.Y - first.Y), second.X - first.X);
        var prodRes = -1 * (second.Y - first.Y) * (third.X - first.X) + (second.X - first.X) * (third.Y - first.Y);

        if (prodRes < 0) v3.Invert();
        return v3;
    }
}