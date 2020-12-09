using System;

namespace RayTracing.Entities
{
    public class Polygon : Entity
    {
        public Point3 P0, P1, P2;
        
        public float A, B, C, D;

        public Material Material { get; }

        private Point3 _normal;

        public Point3 Normal(Point3 point) => _normal;

        public Polygon(Point3 p0, Point3 p1, Point3 p2, Material material, bool reverseNormal = false)
        {
            if (reverseNormal)
            {
                P0 = p2;
                P1 = p1;
                P2 = p0;
            }
            else
            {
                P0 = p0;
                P1 = p1;
                P2 = p2;
            }
            Material = material;
            var x10 = P1.X - P0.X;
            var x20 = P2.X - P0.X;
            var y10 = P1.Y - P0.Y;
            var y20 = P2.Y - P0.Y;
            var z10 = P1.Z - P0.Z;
            var z20 = P2.Z - P0.Z;
            A = y10 * z20 - y20 * z10;
            B = x20 * z10 - x10 * z20;
            C = x10 * y20 - x20 * y10;
            D = - A * P0.X - B * P0.Y - C * P0.Z;
            
            _normal = ((P1 - P0) ^ (P2 - P0)).Normalize();
        }

        public (Point3?, float, Point3?) Intersect(Point3 origin, Point3 direction)
        {
            var coef = A * direction.X + B * direction.Y + C * direction.Z;
            var free = A * origin.X + B * origin.Y + C * origin.Z + D;
            if (Math.Abs(coef) < 0.000001f)
                return (null, -1, null);
            var t = -free / coef;
            if (t < 0)
                return (null, -1, null);
            
            var intersectionPoint = origin + direction * t;
            if (!(InAngle(P0, P1, P2, intersectionPoint)
                  && InAngle(P1, P2, P0, intersectionPoint)
                  && InAngle(P2, P0, P1, intersectionPoint)))
                return (null, -1, null);
            return (intersectionPoint, (intersectionPoint - origin).Len(), _normal);
        }

        private static bool InAngle(Point3 left, Point3 middle, Point3 right, Point3 point)
        {
            var leftVec = left - middle;
            var rightVec = right - middle;
            var pointVec = point - middle;

            var rightAngleCos = rightVec * leftVec / rightVec.Len();
            var pointAngleCos = pointVec * leftVec / pointVec.Len();
            
            return pointAngleCos >= rightAngleCos - 0.0001f;
        }
        
    }
}