namespace RayTracing
{
    public interface Entity
    {
        Material Material { get; }

        (Point3?, float, Point3?) Intersect(Point3 origin, Point3 direction);
    }
}