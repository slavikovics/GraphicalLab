using System.Collections.Generic;
using Avalonia;
using GraphicalLab.Controls.WaypointControl;
using GraphicalLab.Lines;
using GraphicalLab.Models;

namespace GraphicalLab.Poly;

public class Poly
{
    private List<WaypointModel> _points;
    private List<WaypointModel> _edgePoints;
    private bool _closed;
    private Size _canvasSize;

    public Poly(Size canvasSize)
    {
        _points = [];
        _edgePoints = [];
        _canvasSize = canvasSize;
    }

    public Poly(Size canvasSize, List<Pixel> points)
    {
        _points = [];
        _edgePoints = [];
        
        foreach (var point in points)
        {
            var newWaypoint = new WaypointModel { X = point.X / canvasSize.Width, Y = point.Y / canvasSize.Height };
            _points.Add(newWaypoint);
            _edgePoints?.Add(newWaypoint);
        }
           
        _canvasSize = canvasSize;
    }

    public void Clear()
    {
        _points.Clear();
        _edgePoints.Clear();
        _closed = false;
    }

    public void AddPoint(WaypointModel p)
    {
        _points.Add(p);
        _edgePoints.Add(p);
    }
    
    public void AddRange(List<Pixel> points)
    {
        foreach (var p in points)
        {
            var wp = new  WaypointModel { X = p.X / _canvasSize.Width, Y = p.Y / _canvasSize.Height };
            _points.Add(wp);
            _edgePoints.Add(wp);
        }
    }

    public void Close(WaypointModel? model)
    {
        if (model == null || model == _edgePoints[0]) _closed = true;
    }

    public List<Pixel> EdgePointsToPixels()
    {
        var pixels = new List<Pixel>();
        foreach (var p in _edgePoints) pixels.Add(p.ToPixel(_canvasSize));
        return pixels;
    }

    public List<Pixel> Draw()
    {
        List<Pixel> pixels = [];

        for (int i = 1; i < _edgePoints.Count; i++)
        {
            var first = _edgePoints[i - 1].ToPixel(_canvasSize);
            var second = _edgePoints[i].ToPixel(_canvasSize);
            pixels.AddRange(BrezenhemLineGenerator.DrawLine(first, second));
        }

        if (_closed && _edgePoints.Count > 2)
        {
            var first = _edgePoints[^1].ToPixel(_canvasSize);
            var second = _edgePoints[0].ToPixel(_canvasSize);
            pixels.AddRange(BrezenhemLineGenerator.DrawLine(first, second));
        }

        return pixels;
    }

    public ConvexResult Convex()
    {
        var pixels = EdgePointsToPixels();
        return ConvexAnalyzer.FindConvex(pixels);
    }

    public List<Pixel> DrawNorms()
    {
        var pixels = EdgePointsToPixels();
        return NormVectors.DrawArrows(pixels);
    }
}