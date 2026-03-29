using System;
using System.Collections.Generic;
using System.Linq;
using GraphicalLab.Lines;
using GraphicalLab.Models;
using GraphicalLab.Triangulation;

namespace GraphicalLab.Voronoi;

public class VoronoiResult : IDrawable
    {
        public List<Pixel> Draw(List<Pixel> points)
        {
            var sites = points.Select((p, idx) => new Site(new Point(p.X, p.Y)) { Index = idx }).ToList();
            sites.Sort((a, b) =>
            {
                int yComp = a.Coordinate.Y.CompareTo(b.Coordinate.Y);
                return yComp != 0 ? yComp : a.Coordinate.X.CompareTo(b.Coordinate.X);
            });

            var boundingBox = new BoundingBox { XMin = 0, XMax = 300, YMin = 0, YMax = 300 };

            var provider = new ListSiteProvider(sites);
            var collector = new EdgeCollector();

            var algorithm = new FortuneVoronoiAlgorithm(provider, collector, boundingBox);
            algorithm.Compute();

            var allEdges = new List<Edge>(collector.CompletedEdges);
            allEdges.AddRange(algorithm.GetAllEdges());

            var resultPixels = new List<Pixel>();
            foreach (var edge in allEdges)
            {
                var segment = ClipEdge(edge, boundingBox, sites);
                if (segment != null)
                {
                    resultPixels.AddRange(BrezenhemLineGenerator.DrawLine(segment.Value.Start, segment.Value.End));
                }
            }

            return resultPixels;
        }
        
        private (Pixel Start, Pixel End)? ClipEdge(Edge edge, BoundingBox box, List<Site> allSites)
        {
            const double eps = 1e-9;
            const double testDistance = 8.0;

            Pixel ToPixel(Site s) =>
                new Pixel((int)Math.Round(s.Coordinate.X), (int)Math.Round(s.Coordinate.Y));

            double Dist2(Pixel p, Site s)
            {
                double dx = p.X - s.Coordinate.X;
                double dy = p.Y - s.Coordinate.Y;
                return dx * dx + dy * dy;
            }

            bool BelongsToCell(Pixel p, Site owner)
            {
                double dOwner = Dist2(p, owner);

                foreach (var s in allSites)
                {
                    if (ReferenceEquals(s, owner)) continue;
                    if (Dist2(p, s) < dOwner - eps)
                        return false;
                }

                return true;
            }
            
            var s1 = edge.Region[0];
            var s2 = edge.Region[1];
            if (s1 == null || s2 == null)
                return null;

            double dx = s2.Coordinate.X - s1.Coordinate.X;
            double dy = s2.Coordinate.Y - s1.Coordinate.Y;

            double dirX = -dy;
            double dirY = dx;

            double len = Math.Sqrt(dirX * dirX + dirY * dirY);
            if (len < eps) return null;

            dirX /= len;
            dirY /= len;

            Pixel origin;
            if (edge.Endpoint[0] != null)
                origin = ToPixel(edge.Endpoint[0]!);
            else if (edge.Endpoint[1] != null)
                origin = ToPixel(edge.Endpoint[1]!);
            else
            {
                double mx = (s1.Coordinate.X + s2.Coordinate.X) / 2;
                double my = (s1.Coordinate.Y + s2.Coordinate.Y) / 2;
                origin = new Pixel((int)Math.Round(mx), (int)Math.Round(my));
            }

            if (edge.Endpoint[0] != null && edge.Endpoint[1] != null)
            {
                return ClipSegment(ToPixel(edge.Endpoint[0]!), ToPixel(edge.Endpoint[1]!), box);
            }

            if (edge.Endpoint[0] != null || edge.Endpoint[1] != null)
            {
                var known = origin;

                if (known.X < box.XMin - 1 || known.X > box.XMax + 1 ||
                    known.Y < box.YMin - 1 || known.Y > box.YMax + 1)
                    return null;

                double tx1 = known.X + testDistance * dirX;
                double ty1 = known.Y + testDistance * dirY;
                double tx2 = known.X - testDistance * dirX;
                double ty2 = known.Y - testDistance * dirY;

                var testPixel1 = new Pixel((int)Math.Round(tx1), (int)Math.Round(ty1));
                var testPixel2 = new Pixel((int)Math.Round(tx2), (int)Math.Round(ty2));

                bool ok1 = BelongsToCell(testPixel1, s1) || BelongsToCell(testPixel1, s2);
                bool ok2 = BelongsToCell(testPixel2, s1) || BelongsToCell(testPixel2, s2);

                double chosenDirX, chosenDirY;
                if (ok1 && !ok2)
                {
                    chosenDirX = dirX;
                    chosenDirY = dirY;
                }
                else if (ok2 && !ok1)
                {
                    chosenDirX = -dirX;
                    chosenDirY = -dirY;
                }
                else
                {
                    double d1 = (tx1 - known.X) * (tx1 - known.X) + (ty1 - known.Y) * (ty1 - known.Y);
                    double d2 = (tx2 - known.X) * (tx2 - known.X) + (ty2 - known.Y) * (ty2 - known.Y);
                    if (d1 > d2)
                    {
                        chosenDirX = dirX;
                        chosenDirY = dirY;
                    }
                    else
                    {
                        chosenDirX = -dirX;
                        chosenDirY = -dirY;
                    }
                }

                var intersections = new List<(double t, Pixel p)>();

                void TryAddRay(double t)
                {
                    if (t < -eps) return;

                    double x = known.X + t * chosenDirX;
                    double y = known.Y + t * chosenDirY;

                    if (x >= box.XMin - eps && x <= box.XMax + eps &&
                        y >= box.YMin - eps && y <= box.YMax + eps)
                    {
                        var pixel = new Pixel((int)Math.Round(x), (int)Math.Round(y));
                        intersections.Add((t, pixel));
                    }
                }

                if (Math.Abs(chosenDirX) > eps)
                {
                    TryAddRay((box.XMin - known.X) / chosenDirX);
                    TryAddRay((box.XMax - known.X) / chosenDirX);
                }

                if (Math.Abs(chosenDirY) > eps)
                {
                    TryAddRay((box.YMin - known.Y) / chosenDirY);
                    TryAddRay((box.YMax - known.Y) / chosenDirY);
                }

                if (intersections.Count == 0)
                    return null;

                var farthest = intersections.OrderByDescending(item => item.t).First();
                return ClipSegment(known, farthest.p, box);
            }

            double vx = origin.X;
            double vy = origin.Y;

            var fullIntersections = new List<Pixel>();

            void TryAddFull(double t)
            {
                double x = vx + t * dirX;
                double y = vy + t * dirY;
                if (x >= box.XMin - eps && x <= box.XMax + eps &&
                    y >= box.YMin - eps && y <= box.YMax + eps)
                {
                    fullIntersections.Add(new Pixel((int)Math.Round(x), (int)Math.Round(y)));
                }
            }

            if (Math.Abs(dirX) > eps)
            {
                TryAddFull((box.XMin - vx) / dirX);
                TryAddFull((box.XMax - vx) / dirX);
            }

            if (Math.Abs(dirY) > eps)
            {
                TryAddFull((box.YMin - vy) / dirY);
                TryAddFull((box.YMax - vy) / dirY);
            }

            fullIntersections = fullIntersections.Distinct().ToList();
            if (fullIntersections.Count < 2) return null;

            return (fullIntersections[0], fullIntersections[1]);
        }

        private (Pixel, Pixel)? ClipSegment(Pixel p1, Pixel p2, BoundingBox box)
        {
            int code1 = ComputeOutCode(p1, box);
            int code2 = ComputeOutCode(p2, box);
            double x1 = p1.X, y1 = p1.Y;
            double x2 = p2.X, y2 = p2.Y;

            while (true)
            {
                if ((code1 | code2) == 0)
                {
                    return (new Pixel((int)Math.Round(x1), (int)Math.Round(y1)),
                        new Pixel((int)Math.Round(x2), (int)Math.Round(y2)));
                }

                if ((code1 & code2) != 0)
                {
                    return null;
                }

                int codeOut = code1 != 0 ? code1 : code2;
                double x, y;

                if ((codeOut & 8) != 0)
                {
                    x = x1 + (x2 - x1) * (box.YMax - y1) / (y2 - y1);
                    y = box.YMax;
                }
                else if ((codeOut & 4) != 0)
                {
                    x = x1 + (x2 - x1) * (box.YMin - y1) / (y2 - y1);
                    y = box.YMin;
                }
                else if ((codeOut & 2) != 0)
                {
                    y = y1 + (y2 - y1) * (box.XMax - x1) / (x2 - x1);
                    x = box.XMax;
                }
                else
                {
                    y = y1 + (y2 - y1) * (box.XMin - x1) / (x2 - x1);
                    x = box.XMin;
                }

                if (codeOut == code1)
                {
                    x1 = x;
                    y1 = y;
                    code1 = ComputeOutCode(new Pixel((int)Math.Round(x), (int)Math.Round(y)), box);
                }
                else
                {
                    x2 = x;
                    y2 = y;
                    code2 = ComputeOutCode(new Pixel((int)Math.Round(x), (int)Math.Round(y)), box);
                }
            }
        }

        private int ComputeOutCode(Pixel p, BoundingBox box)
        {
            int code = 0;
            if (p.X < box.XMin) code |= 1;
            else if (p.X > box.XMax) code |= 2;
            if (p.Y < box.YMin) code |= 4;
            else if (p.Y > box.YMax) code |= 8;
            return code;
        }
    }