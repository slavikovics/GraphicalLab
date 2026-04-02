using GraphicalLab.Models;

namespace GraphicalLab.Triangulation;

public class Edge
{
    public readonly Pixel A;
    public readonly Pixel B;
        
    public Edge(Pixel a, Pixel b)
    {
        if (ComparePixels(a, b) > 0)
        {
            A = a;
            B = b;
        }
        else
        {
            A = b;
            B = a;
        }
    }
        
    private static int ComparePixels(Pixel p1, Pixel p2)
    {
        if (p1.X != p2.X) return p1.X.CompareTo(p2.X);
        return p1.Y.CompareTo(p2.Y);
    }
        
    public override bool Equals(object? obj)
    {
        if (obj is Edge other)
        {
            return A == other.A && B == other.B;
        }
        return false;
    }
        
    public override int GetHashCode()
    {
        return (A.X * 73856093) ^ (A.Y * 19349663) ^ 
               (B.X * 83492791) ^ (B.Y * 19349663);
    }
        
    public override string ToString()
    {
        return $"({A.X},{A.Y}) - ({B.X},{B.Y})";
    }
}