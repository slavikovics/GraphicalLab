using System.Collections.Generic;
using GraphicalLab.Lines;
using GraphicalLab.Models;

namespace GraphicalLab.Triangulation;

public class Triangle
{
    public readonly Pixel V1, V2, V3;
        
    public Triangle(Pixel v1, Pixel v2, Pixel v3)
    {
        V1 = v1;
        V2 = v2;
        V3 = v3;
    }

    public List<Pixel> Draw()
    {
        var result = new List<Pixel>();
        result.AddRange(BrezenhemLineGenerator.DrawLine(V1, V2));
        result.AddRange(BrezenhemLineGenerator.DrawLine(V2, V3));
        result.AddRange(BrezenhemLineGenerator.DrawLine(V3, V1));
        return result;
    }
        
    public override string ToString()
    {
        return $"({V1.X},{V1.Y}) - ({V2.X},{V2.Y}) - ({V3.X},{V3.Y})";
    }
}