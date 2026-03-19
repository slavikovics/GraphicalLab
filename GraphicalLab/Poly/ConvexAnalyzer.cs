using System.Collections.Generic;
using GraphicalLab.Models;

namespace GraphicalLab.Poly;

public class ConvexAnalyzer
{
    public static ConvexResult FindConvex(List<Pixel> pixels)
    {
        if (pixels.Count < 3) return ConvexResult.AllZero;
    
        bool? isPositive = null;
    
        for (int i = 0; i < pixels.Count; i++)
        {
            var first = pixels[i];
            var second = pixels[(i + 1) % pixels.Count];
            var third = pixels[(i + 2) % pixels.Count];
        
            var result = (second.X - first.X) * (third.Y - second.Y) - 
                         (third.X - second.X) * (second.Y - first.Y);
        
            if (result > 0)
            {
                if (isPositive == false) return ConvexResult.Mixed;
                isPositive = true;
            }
            else if (result < 0)
            {
                if (isPositive == true) return ConvexResult.Mixed;
                isPositive = false;
            }
        }
    
        if (isPositive == null) return ConvexResult.AllZero;
        return isPositive.Value ? ConvexResult.AllPositive : ConvexResult.AllNegative;
    }
}