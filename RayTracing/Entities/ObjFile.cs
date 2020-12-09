using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;

namespace RayTracing.Entities
{
    public class ObjFile : Entity
    {
        private List<Polygon> Polygons = new List<Polygon>();

        public Material Material { get; }
        

        public ObjFile(string fname, float scale, Point3 transform, Material material)
        {
            LoadFromObj(File.ReadAllLines(fname), scale, transform);
            Material = material;
        }

        public (Point3?, float, Point3?) Intersect(Point3 origin, Point3 direction)
        {
            var resPoint = (Point3?) null;
            var minDistance = float.PositiveInfinity;
            var resNormal = (Point3?) null;
            foreach (var poly in Polygons)
            {
                var (point, distance, normal) = poly.Intersect(origin, direction);
                if (point != null && distance < minDistance)
                {
                    resPoint = point;
                    minDistance = distance;
                    resNormal = normal;
                }
            }
            return (resPoint, minDistance, resNormal);
        }
        
        private void LoadFromObj(IEnumerable<string> file, float scale, Point3 transform)
        {
            var points = new List<Point3>();
            foreach (var line in file)
            {
                if (line.Length == 0)
                    continue;
                if (line[0] == '#')
                    continue;
                var split = line.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                if (split[0] == "v")
                    points.Add(ParsePoint(split, scale, transform));
                else if (split[0] == "f")
                    ParsePolygon(split, points);
            }
        }

        private static Point3 ParsePoint(IReadOnlyList<string> line, float scale, Point3 transform)
            => new Point3(ParseFloat(line[1]), ParseFloat(line[2]), ParseFloat(line[3])) * scale + transform;

        private void ParsePolygon(string[] line, List<Point3> points)
        {
            var pointIndices = new List<int>();
            foreach (var str in line.Skip(1))
            {
                var pointIdx = int.Parse(str.Substring(0, str.IndexOf('/')));
                if (pointIdx < 0)
                    pointIdx = -pointIdx;
                pointIdx -= 1;
                pointIndices.Add(pointIdx);
            }

            for (var i = 1; i < pointIndices.Count - 1; i++)
                Polygons.Add(new Polygon(
                    points[pointIndices[0]],
                    points[pointIndices[i]],
                    points[pointIndices[i + 1]],
                    new Material(Color.Black)));
        }

        private static float ParseFloat(string str)
            => float.Parse(str, CultureInfo.InvariantCulture);
    }
}