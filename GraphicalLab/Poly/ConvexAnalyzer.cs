using System;
using System.Collections.Generic;
using GraphicalLab.Models;

namespace GraphicalLab.Poly;

public class ConvexAnalyzer
{
    public static SignType FindSignType(Pixel first, Pixel second, Pixel third)
    {
        var result = (second.X - first.X) * (third.Y - second.Y) - (third.X - second.X) * (second.Y - first.Y);
        if (result == 0) return SignType.Zero;
        if (result > 0) return SignType.Positive;
        return SignType.Negative;
    }

    public static ConvexResult FindConvex(List<Pixel> pixels)
    {
        if (pixels.Count < 3) return ConvexResult.AllZero;
        List<SignType> signTypes = [];
        
        for (int i = 0; i < pixels.Count; i++)
        {
            var first = pixels[i];
            var second = pixels[(i + 1) % pixels.Count];
            var third = pixels[(i + 2) % pixels.Count];
            signTypes.Add(FindSignType(first, second, third));
        }

        int zeroCount = 0;
        int negativeCount = 0;
        int positiveCount = 0;
        foreach (var type in signTypes)
        {
            if (type == SignType.Zero) zeroCount++;
            else if (type == SignType.Positive) positiveCount++;
            else negativeCount++;
        }

        if (zeroCount == signTypes.Count) return ConvexResult.AllZero;
        if (positiveCount == signTypes.Count) return ConvexResult.AllPositive;
        if (negativeCount == signTypes.Count) return ConvexResult.AllNegative;
        return ConvexResult.Mixed;
    }
}