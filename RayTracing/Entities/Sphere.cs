using System;

namespace RayTracing.Entities
{
    public class Sphere : Entity
    {
        private Point3 _center;

        private float _r;

        private bool _reverseNormal;

        public Material Material { get; }
        
        public Sphere(Point3 center, float r, Material material, bool reverseNormal = false)
        {
            _center = center;
            _r = r;
            Material = material;
            _reverseNormal = reverseNormal;
        }

        private Point3 Normal(Point3 point)
            => !_reverseNormal ? (_center - point).Normalize() : (point - _center).Normalize();

        public (Point3?, float, Point3?) Intersect(Point3 origin, Point3 direction)
        {
            origin -= _center;
            var a = (double)(direction * direction);
            var b = 2.0 * (origin * direction);
            var c = origin * origin - (double)_r * _r;
            var d = b * b - 4 * a * c;
            if (d < 0)
                return (null, -1, null);
            d = Math.Sqrt(d);
            var t1 = (-b - d) / (a * 2);
            var t2 = (-b + d) / (a * 2);
            if (t1 < 0) t1 = double.PositiveInfinity;
            if (t2 < 0) t2 = double.PositiveInfinity;
            t1 = t1 < t2 ? t1 : t2;
            if (double.IsPositiveInfinity(t1))
                return (null, -1, null);

            var delta = direction * (float)t1;
            var point = _center + origin + delta;
            return (point, delta.Len(), Normal(point));
        }
    }
}