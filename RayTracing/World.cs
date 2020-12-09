using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using RayTracing.Entities;

namespace RayTracing
{
    public class World
    {
        public List<Entity> Polygons;

        private const float Ka = 1f;

        private const float Ia = 0.1f;

        private const float Ks = 0.3f;

        private const float Kd = 1f;

        private const float Sh = 80;

        private const int MaxDepth = 10;
        
        private const float FOV = (float)Math.PI / 1.5f;
        
        public List<Point3> Light = new List<Point3>();

        public World()
        {
            Light.Add(new Point3(5, -7, 13));
            Light.Add(new Point3(-5, -5, 1));
            Polygons = Scene1();
        }

        public Bitmap RayTrace(int width, int height)
        {
            var bitmap = new Bitmap(width, height);
            var drawer = Graphics.FromImage(bitmap);
            drawer.Clear(Color.Black);
            var matrix = new Point3[width * height];
            Enumerable.Range(0, height).GroupBy(j => j / (height / 4)).AsParallel().ForAll(g =>
            {
                foreach (var i in Enumerable.Range(0, width))
                {
                    foreach (var j in g)
                    {
                        matrix[i * height + j] = TraceOneRay(
                            new Point3(0, 0, 0),
                            VecForAngle(
                                (i - width / 2f) / width * FOV,
                                (j - height / 2f) / height * FOV)
                        );
                    }
                }
            });


            for (var i = 0; i < width; i++)
            for (var j = 0; j < height; j++)
                bitmap.SetPixel(i, j, matrix[i * height + j].ToColor());
            return bitmap;
        }

        private static Point3 VecForAngle(float x, float y)
            => new Point3((float)Math.Sin(x), y, (float)Math.Cos(x));

        private Point3 TraceOneRay(Point3 origin, Point3 vec, int depth = 0)
        {
            var (polyIdx, polyPoint, _, normal) = CastRay(origin, vec);

            if (polyIdx == -1) return new Point3(0, 0, 0);

            var material = Polygons[polyIdx].Material;
            var color = new Point3(material.Color);

            var shade = 0.0f;
            foreach (var light in Light)
            {
                var shadowed = CastRay(polyPoint, light - polyPoint).distance < (light - polyPoint).Len();
                if (!shadowed)
                {
                    var lightVector = (polyPoint - light).Normalize();
                    shade += Ka * Ia +
                             Kd * (normal * lightVector) +
                             Ks * (float) Math.Pow(normal * Reflection(lightVector, normal), Sh);
                }
                else
                    shade += Ka * Ia;
            }
            color *= (shade / Light.Count);

            if (depth >= MaxDepth) 
                return color;
            
            if (material.Mirroring > 0)
            {
                var mirror = TraceOneRay(polyPoint, Reflection(vec, normal), depth + 1);
                color = color * (1 - material.Mirroring) + mirror * material.Mirroring;
            }
            
            if (material.Transparency > 0)
            {
                var visibleThrough =
                    TraceOneRay(polyPoint, Refraction(vec, normal, material.Koef), depth + 1);
                color = color * (1 - material.Transparency) + visibleThrough * material.Transparency;
            }

            return color;
        }

        private static Point3 Reflection(Point3 vec, Point3 normal)
            => vec - normal * (2 * (normal * vec));

        private static Point3 Refraction(Point3 vec, Point3 normal, float koef)
        {
            vec = vec.Normalize();
            var n1 = vec.Len();
            var n2 = koef;
            return vec + ((float) Math.Sqrt((n2 * n2 - n1 * n1) / Math.Pow(vec * normal, 2) + 1) - 1) * (vec * normal) *
                normal;
        }

        private (int polygon, Point3 point, float distance, Point3 normal) CastRay(Point3 origin, Point3 vec)
        {
            origin += vec * 0.00001f;
            var minDist = float.PositiveInfinity;
            var polyIdx = -1;
            var polyPoint = new Point3(0, 0, 0);
            var polyNormal = new Point3(0, 0, 0);
            for (var poly = 0; poly < Polygons.Count; poly++)
            {
                var (point, distance, normal) = Polygons[poly].Intersect(origin, vec);
                if (point == null || !(distance < minDist))
                    continue;
                minDist = distance;
                polyIdx = poly;
                polyPoint = (Point3) point;
                polyNormal = (Point3) normal;
            }

            return (polyIdx, polyPoint, minDist, polyNormal);
        }

        private static List<Entity> Scene2()
        {
            
            var polygons = new List<Entity>();
            polygons.Add(new Sphere(new Point3(0, 0, 0), 50, new Material(Color.Aqua, 0.5f), true));
            for (var i = -Math.PI; i < Math.PI; i += Math.PI / 6)
                polygons.Add(new Sphere(
                    new Point3((float)Math.Sin(i) * 20, 0, (float)Math.Cos(i) * 20 - 10), 3,
                    new Material(Color.Red, 0.5f)));
            for (var i = -Math.PI; i < Math.PI; i += Math.PI / 6)
                polygons.Add(new Sphere(
                    new Point3(0, (float)Math.Sin(i) * 15, (float)Math.Cos(i) * 15 + 5), 1.5f,
                    new Material(Color.Blue, 0.5f)));
            return polygons;
        }

        private static List<Entity> Scene1()
        {
            var polygons = MakeCube(
                new Point3(-10, -10, -1),
                new Point3(10, 10, 20),
                new List<Material>
                {
                    new Material(Color.Blue),
                    new Material(Color.Orange),
                    new Material(Color.Green, 0.5f),
                    new Material(Color.White),
                    new Material(Color.White),
                    new Material(Color.Purple, 0.5f),
                },
                true);
            polygons.AddRange(RelativeCube(
                new Point3(-6, 1, 7),
                new Point3(4, 9, 4),
                new Material(Color.Red, 0.5f)));
            
            polygons.AddRange(RelativeCube(
                new Point3(0, 3, 12),
                new Point3(2, 7, 2),
                new Material(Color.Brown)));

            polygons.AddRange(MakeCube(
                new Point3(-10, 5, -1),
                new Point3(10, 10, 20),
                new Material(Color.Cyan, 0.1f, 0.5f, 1.33f)));
            
            polygons.Add(new Sphere(
                new Point3(-4, -2, 9), 3,
                new Material(Color.DarkGreen, 0.0f, 0.7f, 1.33f)));
            
            polygons.Add(new Sphere(
                new Point3(1, 2.5f, 13), 0.5f,
                new Material(Color.Moccasin)));
            
            polygons.Add(new Sphere(
                new Point3(15, 5, 14), 10,
                new Material(Color.Gold, 0.3f)));

            for (var i = 0; i < 5; i++)
                polygons.Add(new Sphere(
                    new Point3(-8 + i * 4, -8, 18), 2,
                    new Material(Color.Magenta, i / 5f)));
            return polygons;
        }

        private static List<Entity> RelativeCube(Point3 start, Point3 size, Material material)
            => MakeCube(start, new Point3(start.X + size.X, start.Y + size.Y, start.Z + size.Z), material);

        private static List<Entity> MakeCube(Point3 p000, Point3 p111, Material material, bool reverseNormals = false)
            => MakeCube(p000, p111, Enumerable.Range(0, 6).Select(_ => material).ToList(), reverseNormals);

        private static List<Entity> MakeCube(Point3 p000, Point3 p111, IReadOnlyList<Material> material,
            bool reverseNormals = false)
        {
            var p001 = new Point3(p000.X, p000.Y, p111.Z);
            var p010 = new Point3(p000.X, p111.Y, p000.Z);
            var p100 = new Point3(p111.X, p000.Y, p000.Z);
            var p011 = new Point3(p000.X, p111.Y, p111.Z);
            var p101 = new Point3(p111.X, p000.Y, p111.Z);
            var p110 = new Point3(p111.X, p111.Y, p000.Z);
            return new List<Entity>
            {
                new Polygon(p000, p100, p010, material[0], reverseNormals),
                new Polygon(p100, p110, p010, material[0], reverseNormals),

                new Polygon(p010, p110, p011, material[1], reverseNormals),
                new Polygon(p110, p111, p011, material[1], reverseNormals),

                new Polygon(p110, p100, p111, material[2], reverseNormals),
                new Polygon(p100, p101, p111, material[2], reverseNormals),

                new Polygon(p001, p101, p000, material[3], reverseNormals),
                new Polygon(p101, p100, p000, material[3], reverseNormals),

                new Polygon(p000, p001, p010, material[4], !reverseNormals),
                new Polygon(p001, p011, p010, material[4], !reverseNormals),

                new Polygon(p011, p111, p001, material[5], reverseNormals),
                new Polygon(p111, p101, p001, material[5], reverseNormals),
            };
        }
    }
}