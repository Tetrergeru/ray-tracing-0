using System.Drawing;

namespace RayTracing
{
    public struct Material
    {
        public Color Color;

        public readonly float Mirroring;

        public readonly float Transparency;

        public readonly float Koef;
        
        public Material(Color color, float mirroring = 0, float transparency = 0, float koef = 1)
        {
            Color = color;
            Koef = koef;
            Transparency = transparency;
            Mirroring = mirroring;
        }
    }
}