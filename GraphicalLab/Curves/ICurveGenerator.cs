using System.Collections.Generic;
using Avalonia;
using GraphicalLab.Controls.WaypointControl;
using GraphicalLab.Models;

namespace GraphicalLab.Curves;

public interface ICurveGenerator
{
    List<Pixel> Draw(List<Point> waypoints, uint color = 4278190335);
}