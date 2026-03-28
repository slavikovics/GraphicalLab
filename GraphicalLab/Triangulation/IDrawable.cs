using System.Collections.Generic;
using GraphicalLab.Models;

namespace GraphicalLab.Triangulation;

public interface IDrawable
{
    List<Pixel> Draw(List<Pixel> points);
}