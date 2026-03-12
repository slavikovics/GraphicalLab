using System.Collections.Generic;
using Avalonia;
using GraphicalLab.Controls.WaypointControl;
using GraphicalLab.Lines;
using GraphicalLab.Models;

namespace GraphicalLab.Poly;

public class Poly
{
    private List<WaypointModel> _points;
    private bool _closed;
    private Size _canvasSize;

    public Poly(Size canvasSize)
    {
        _points = [];
        _canvasSize = canvasSize;
    }

    public void Clear()
    {
        _points.Clear();
        _closed = false;
    }

    public void AddPoint(WaypointModel p)
    {
        _points.Add(p);
    }

    public void Close(WaypointModel? model)
    {
        if (model != _points[0]) return;
        _closed = true;
    }

    public List<Pixel> Draw()
    {
        List<Pixel> pixels = [];

        for (int i = 1; i < _points.Count; i++)
        {
            var first = _points[i - 1].ToPixel(_canvasSize);
            var second = _points[i].ToPixel(_canvasSize);
            pixels.AddRange(BrezenhemLineGenerator.DrawLine(first, second));
        }

        if (_closed && _points.Count > 2)
        {
            var first = _points[^1].ToPixel(_canvasSize);
            var second = _points[0].ToPixel(_canvasSize);
            pixels.AddRange(BrezenhemLineGenerator.DrawLine(first, second));
        }

        return pixels;
    }
}