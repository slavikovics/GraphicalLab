using System.Collections.Generic;
using Avalonia;
using GraphicalLab.Controls.WaypointControl;
using GraphicalLab.Lines;
using GraphicalLab.Models;

namespace GraphicalLab.Curves;

public class Curve
{
    private List<WaypointModel> Waypoints { get; set; }
    private List<Pixel> Pixels { get; set; }
    private readonly Size _canvasSize;
    private readonly ICurveGenerator _curveGenerator;

    public Curve(List<WaypointModel> waypoints, Size canvasSize, ICurveGenerator generator)
    {
        Waypoints = waypoints;
        _canvasSize = canvasSize;
        _curveGenerator = generator;
        
        Pixels = [];
    }

    public List<Point> GetPoints()
    {
        var points = new List<Point>();
        foreach (var waypoint in Waypoints)
            points.Add(waypoint.GetAbsolutePosition(_canvasSize));
        
        return points;
    }

    public bool HasWaypoint(WaypointModel waypointModel)
    {
        return Waypoints.Contains(waypointModel);
    }

    public List<Pixel> Draw()
    {
        Pixels = _curveGenerator.Draw(GetPoints());
        return Pixels;
    }

    public static List<Curve>? CreateCurves(List<WaypointModel> wayPoints, Size canvasSize, ICurveGenerator generator)
    {
        if (wayPoints.Count < 4) return null;
        List<Curve> curves = [];

        if (generator is not Spline)
        {
            for (int i = 0; i <= wayPoints.Count - 4; i += 3)
            {
                var firstWaypoint = wayPoints[i];
                var secondWaypoint = wayPoints[i + 1];
                var thirdWaypoint = wayPoints[i + 2];
                var fourthWaypoint = wayPoints[i + 3];

                var curve = new Curve([firstWaypoint, secondWaypoint, thirdWaypoint, fourthWaypoint], canvasSize, generator);
                curves.Add(curve);
            }

            return curves;
        }

        curves.Add(new Curve(wayPoints, canvasSize, generator));
        return curves;
    }
}