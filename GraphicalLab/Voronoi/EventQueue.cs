using System;
using System.Diagnostics;

namespace GraphicalLab.Voronoi;

public class EventQueue
    {
        private readonly double _yMin;
        private readonly double _yMax;
        private readonly double _deltaY;
        private readonly int _hashSize;
        private readonly HalfEdge[] _hashTable;
        private int _minBucket;
        private int _count;

        public EventQueue(BoundingBox boundingBox, int sqrtSiteCount)
        {
            _yMin = boundingBox.YMin;
            _yMax = boundingBox.YMax;
            _deltaY = boundingBox.YMax - boundingBox.YMin;
            _hashSize = 4 * sqrtSiteCount;
            _hashTable = new HalfEdge[_hashSize];
            for (int i = 0; i < _hashSize; i++) _hashTable[i] = new HalfEdge();
            _minBucket = 0;
            _count = 0;
        }

        private int GetBucket(HalfEdge halfEdge)
        {
            int bucket;
            if (halfEdge.YStar < _yMin) bucket = 0;
            else if (halfEdge.YStar >= _yMax) bucket = _hashSize - 1;
            else bucket = (int)((halfEdge.YStar - _yMin) / _deltaY * _hashSize);
            bucket = Math.Clamp(bucket, 0, _hashSize - 1);
            if (bucket < _minBucket) _minBucket = bucket;
            return bucket;
        }

        public void Insert(HalfEdge halfEdge, Site vertex, double offset)
        {
            halfEdge.Vertex = vertex;
            halfEdge.YStar = vertex.Coordinate.Y + offset;

            int bucket = GetBucket(halfEdge);
            HalfEdge last = _hashTable[bucket];
            HalfEdge? next = last.PriorityQueueNext;

            while (next != null &&
                   (halfEdge.YStar > next.YStar ||
                    (Math.Abs(halfEdge.YStar - next.YStar) < 1e-10 && vertex.Coordinate.X > next.Vertex!.Coordinate.X)))
            {
                last = next;
                next = next.PriorityQueueNext;
            }

            halfEdge.PriorityQueueNext = last.PriorityQueueNext;
            last.PriorityQueueNext = halfEdge;
            _count++;
            Log($"Inserted event at y={halfEdge.YStar:F2} (bucket {bucket})");
        }

        public void Delete(HalfEdge halfEdge)
        {
            if (halfEdge.Vertex != null)
            {
                int bucket = GetBucket(halfEdge);
                HalfEdge last = _hashTable[bucket];
                while (last.PriorityQueueNext != halfEdge) last = last.PriorityQueueNext!;
                last.PriorityQueueNext = halfEdge.PriorityQueueNext;
                _count--;
                halfEdge.Vertex = null;
                Log($"Deleted event at bucket {bucket}");
            }
        }

        public bool IsEmpty => _count == 0;

        public Point GetMinPoint()
        {
            while (_hashTable[_minBucket].PriorityQueueNext == null) _minBucket++;
            HalfEdge he = _hashTable[_minBucket].PriorityQueueNext!;
            return new Point(he.Vertex!.Coordinate.X, he.YStar);
        }

        public HalfEdge ExtractMin()
        {
            HalfEdge curr = _hashTable[_minBucket].PriorityQueueNext!;
            _hashTable[_minBucket].PriorityQueueNext = curr.PriorityQueueNext;
            _count--;
            Log($"Extracted min event at y={curr.YStar:F2}");
            return curr;
        }

        private static void Log(string message) => Debug.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
    }