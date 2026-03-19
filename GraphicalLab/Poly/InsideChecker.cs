using System.Collections.Generic;
using System.Linq;
using GraphicalLab.Models;

namespace GraphicalLab.Poly;

public static class InsideChecker
{
    public static bool IsInside(Pixel pixel, List<Pixel> vertices)
    {
        if (vertices.Count < 3) return false;
        if (vertices.Any(v => v.X == pixel.X && v.Y == pixel.Y))
            return true;
        
        var inside = false;
        for (int i = 0; i < vertices.Count; i++)
        {
            int j = (i + 1) % vertices.Count;
            if (vertices[i].Y > pixel.Y == vertices[j].Y > pixel.Y) continue;
            if (vertices[j].Y == vertices[i].Y) continue;
            
            var intersectX = vertices[i].X + (pixel.Y - vertices[i].Y) * (vertices[j].X - vertices[i].X) /
                (vertices[j].Y - vertices[i].Y);
            if (intersectX >= pixel.X) inside = !inside;
        }

        return inside;
    }
}