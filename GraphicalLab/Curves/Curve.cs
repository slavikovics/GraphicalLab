using System.Collections.Generic;
using Avalonia;
using GraphicalLab.Controls.WaypointControl;
using GraphicalLab.Lines;
using GraphicalLab.Models;

namespace GraphicalLab.Curves;

public class Curve
{
    public WaypointModel FirstWaypoint { get; set; }
    public WaypointModel SecondWaypoint { get; set; }
    public List<Pixel> Pixels { get; set; }
    private Size _canvasSize;

    public Curve(WaypointModel firstWaypoint, WaypointModel secondWaypoint, Size canvasSize)
    {
        FirstWaypoint = firstWaypoint;
        SecondWaypoint = secondWaypoint;
        _canvasSize = canvasSize;
    }

    public List<Pixel> Draw()
    {
        var startPoint = FirstWaypoint.GetAbsolutePosition(_canvasSize);
        var endPoint = SecondWaypoint.GetAbsolutePosition(_canvasSize);
        var startPixel = new Pixel(startPoint.X, startPoint.Y);
        var endPixel = new Pixel(endPoint.X, endPoint.Y);
        Pixels = XiaolinWuLineGenerator.DrawLine(startPixel, endPixel);

        return Pixels;
    }

    public static List<Curve>? CreateSpline(List<WaypointModel> wayPoints, Size canvasSize)
    {
        if (wayPoints.Count < 2) return null;
        List<Curve> curves = [];
        
        for (int i = 0; i < wayPoints.Count - 1; i++)
        {
            var firstWaypoint = wayPoints[i];
            var secondWaypoint = wayPoints[i + 1];
            
            var curve = new Curve(firstWaypoint, secondWaypoint, canvasSize);
            curves.Add(curve);
        }

        return curves;
    }
}