using System.Collections.Generic;
using GraphicalLab.Models;

namespace GraphicalLab.Triangulation;

public class TriangulationResult : IDrawable
{
    public List<Pixel> Draw(List<Pixel> points)
    {
        return Triangulation.DrawTriangulation(points);
    }
}