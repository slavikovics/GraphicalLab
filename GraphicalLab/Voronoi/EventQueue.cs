using System;
using System.Diagnostics;

namespace GraphicalLab.Voronoi;

public class EventQueue
{
    private HalfEdge? _head;
    private int _count;

    public EventQueue(BoundingBox boundingBox, int sqrtSiteCount)
    {
        _head = null;
        _count = 0;
    }

    public void Insert(HalfEdge halfEdge, Site vertex, double offset)
    {
        halfEdge.Vertex = vertex;
        halfEdge.YStar = vertex.Coordinate.Y + offset;
        halfEdge.PriorityQueueNext = null;

        if (_head == null)
        {
            _head = halfEdge;
            _count++;
            Log($"Inserted event at y={halfEdge.YStar:F2} (first event)");
            return;
        }

        HalfEdge? current = _head;
        HalfEdge? previous = null;

        while (current != null &&
               (halfEdge.YStar > current.YStar ||
                (Math.Abs(halfEdge.YStar - current.YStar) < 1e-10 &&
                 vertex.Coordinate.X > current.Vertex!.Coordinate.X)))
        {
            previous = current;
            current = current.PriorityQueueNext;
        }

        if (previous == null)
        {
            halfEdge.PriorityQueueNext = _head;
            _head = halfEdge;
        }
        else
        {
            halfEdge.PriorityQueueNext = previous.PriorityQueueNext;
            previous.PriorityQueueNext = halfEdge;
        }

        _count++;
        Log($"Inserted event at y={halfEdge.YStar:F2}");
    }

    public void Delete(HalfEdge halfEdge)
    {
        if (halfEdge.Vertex == null) return;

        if (_head == halfEdge)
        {
            _head = halfEdge.PriorityQueueNext;
            halfEdge.Vertex = null;
            _count--;
            Log($"Deleted event (was head)");
            return;
        }

        HalfEdge? current = _head;
        while (current != null && current.PriorityQueueNext != halfEdge)
            current = current.PriorityQueueNext;

        if (current != null)
        {
            current.PriorityQueueNext = halfEdge.PriorityQueueNext;
            halfEdge.Vertex = null;
            _count--;
            Log($"Deleted event");
        }
    }

    public bool IsEmpty => _count == 0;

    public Point GetMinPoint()
    {
        if (_head == null) throw new InvalidOperationException("Queue is empty");
        return new Point(_head.Vertex!.Coordinate.X, _head.YStar);
    }

    public HalfEdge ExtractMin()
    {
        if (_head == null) throw new InvalidOperationException("Queue is empty");

        HalfEdge result = _head;
        _head = _head.PriorityQueueNext;
        _count--;

        Log($"Extracted min event at y={result.YStar:F2}");
        return result;
    }

    private static void Log(string message) => Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
}