using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GraphicalLab.Points;

namespace GraphicalLab.Services.ObjToFigureConverter
{
    public class ObjToFigureConverter : IObjToFigureConverter
    {
        public async Task<Figure> ConvertFromFileAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"OBJ файл не найден: {filePath}");

            var lines = await File.ReadAllLinesAsync(filePath);
            return ParseObjContent(lines);
        }

        private Figure ParseObjContent(string[] lines)
        {
            var vertices = new List<Point3>();
            var faces = new List<List<int>>();

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                    continue;

                var parts = trimmedLine.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0)
                    continue;

                switch (parts[0])
                {
                    case "v":
                        if (parts.Length >= 4)
                        {
                            if (double.TryParse(parts[1], System.Globalization.NumberStyles.Any,
                                    System.Globalization.CultureInfo.InvariantCulture, out var x) &&
                                double.TryParse(parts[2], System.Globalization.NumberStyles.Any,
                                    System.Globalization.CultureInfo.InvariantCulture, out var y) &&
                                double.TryParse(parts[3], System.Globalization.NumberStyles.Any,
                                    System.Globalization.CultureInfo.InvariantCulture, out var z))
                            {
                                vertices.Add(new Point3(x, y, z));
                            }
                        }

                        break;

                    case "f":
                        var faceVertices = new List<int>();
                        for (int i = 1; i < parts.Length; i++)
                        {
                            var vertexPart = parts[i].Split('/')[0];
                            if (int.TryParse(vertexPart, out var vertexIndex))
                            {
                                faceVertices.Add(vertexIndex > 0 ? vertexIndex - 1 : vertices.Count + vertexIndex);
                            }
                        }

                        if (faceVertices.Count >= 3)
                        {
                            faces.Add(faceVertices);
                        }

                        break;
                }
            }
            
            var linesList = new List<Line>();
            var edgeSet = new HashSet<(int, int)>();

            foreach (var face in faces)
            {
                for (int i = 0; i < face.Count; i++)
                {
                    int v1 = face[i];
                    int v2 = face[(i + 1) % face.Count];
                    
                    if (v1 > v2)
                    {
                        (v1, v2) = (v2, v1);
                    }
                    
                    if (edgeSet.Add((v1, v2)))
                    {
                        if (v1 >= 0 && v1 < vertices.Count && v2 >= 0 && v2 < vertices.Count)
                        {
                            linesList.Add(new Line(vertices[v1], vertices[v2]));
                        }
                    }
                }
            }
            
            CenterFigure(vertices, linesList);

            return new Figure(new List<Point3>(), linesList);
        }

        private void CenterFigure(List<Point3> vertices, List<Line> lines)
        {
            if (vertices.Count == 0)
                return;
            
            double centerX = vertices.Average(v => v.X);
            double centerY = vertices.Average(v => v.Y);
            double centerZ = vertices.Average(v => v.Z);
            
            var offset = new Point3(-centerX, -centerY, -centerZ);
            
            for (int i = 0; i < vertices.Count; i++)
            {
                vertices[i] = new Point3(
                    vertices[i].X + offset.X,
                    vertices[i].Y + offset.Y,
                    vertices[i].Z + offset.Z
                );
            }
        }
    }
}