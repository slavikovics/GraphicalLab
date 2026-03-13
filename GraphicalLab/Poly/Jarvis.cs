using System.Collections.Generic;
using System.Linq;
using GraphicalLab.Models;

namespace GraphicalLab.Poly;

public class Jarvis
{
    public static List<Pixel> Draw(List<Pixel> points)
    {
        if (points.Count < 3) return [];
    
        List<Pixel> hull = new List<Pixel>();
    
        Pixel start = points.OrderBy(p => p.Y).ThenBy(p => p.X).First();
        Pixel current = start;
    
        do
        {
            hull.Add(current);
        
            Pixel next = points[0];
        
            foreach (var candidate in points)
            {
                if (candidate == current)
                    continue;
            
                int cross = (next.X - current.X) * (candidate.Y - current.Y) - 
                            (next.Y - current.Y) * (candidate.X - current.X);
            
                if (next == current || cross > 0 ||
                    (cross == 0 && DistanceSquared(current, candidate) > DistanceSquared(current, next)))
                {
                    next = candidate;
                }
            }
        
            current = next;
        
        } while (current != start);
    
        return hull;
    }

    private static int DistanceSquared(Pixel p1, Pixel p2)
    {
        int dx = p2.X - p1.X;
        int dy = p2.Y - p1.Y;
        return dx * dx + dy * dy;
    }
}