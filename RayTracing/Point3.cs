using System;
using System.Drawing;

namespace RayTracing
{
    public struct Point3
    {
        public float X;

        public float Y;

        public float Z;

        public Point3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Point3(Color color)
        {
            X = color.R;
            Y = color.G;
            Z = color.B;
        }
        
        public static Point3 operator *(float number, Point3 self)
            => new Point3(self.X * number, self.Y * number, self.Z * number);
        
        public static Point3 operator *(Point3 self, float number)
            => new Point3(self.X * number, self.Y * number, self.Z * number);

        public static Point3 operator +(Point3 self, Point3 other)
            => new Point3(self.X + other.X, self.Y + other.Y, self.Z + other.Z);

        public static float operator *(Point3 self, Point3 other)
            => self.X * other.X + self.Y * other.Y + self.Z * other.Z;

        public static Point3 operator -(Point3 self, Point3 other)
            => new Point3(self.X - other.X, self.Y - other.Y, self.Z - other.Z);

        public static Point3 operator ^(Point3 self, Point3 other)
            => new Point3(
                self.Y * other.Z - self.Z * other.Y,
                self.Z * other.X - self.X * other.Z,
                self.X * other.Y - self.Y * other.X);

        public Color ToColor()
            => Color.FromArgb(Bytize(X), Bytize(Y), Bytize(Z));

        public Point3 Normalize() => this * (1 / Len());
        
        private static byte Bytize(float number)
        {
            if (number <= 0)
                number = 0;
            if (number >= 255)
                number = 255;
            return (byte) number;
        }

        public float Len() => (float) Math.Sqrt(this * this);

        public override string ToString()
            => $"({X}, {Y}, {Z})";
    }
}