using System.Windows.Media.Media3D;
using MeshViewer.Models;

namespace MeshViewer.Geometry3D;

public static class AABBUtils
{
    public static bool Intersects(Ray3D ray, BoundingBox bounds)
    {
        var normalizedRay = ray.Normalize();
        var min = new Point3D(bounds.Min.X, bounds.Min.Y, bounds.Min.Z);
        var max = new Point3D(bounds.Max.X, bounds.Max.Y, bounds.Max.Z);

        var tMin = 0d;
        var tMax = double.PositiveInfinity;

        return IntersectAxis(normalizedRay.Origin.X, normalizedRay.Direction.X, min.X, max.X, ref tMin, ref tMax)
            && IntersectAxis(normalizedRay.Origin.Y, normalizedRay.Direction.Y, min.Y, max.Y, ref tMin, ref tMax)
            && IntersectAxis(normalizedRay.Origin.Z, normalizedRay.Direction.Z, min.Z, max.Z, ref tMin, ref tMax);
    }

    public static BoundingBox FromPoints(IEnumerable<Point3D> points)
    {
        using var enumerator = points.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            throw new ArgumentException("At least one point is required.", nameof(points));
        }

        var first = enumerator.Current;
        var bounds = BoundingBox.FromVertex(new Vertex(first.X, first.Y, first.Z));
        while (enumerator.MoveNext())
        {
            var point = enumerator.Current;
            bounds = bounds.Include(new Vertex(point.X, point.Y, point.Z));
        }

        return bounds;
    }

    private static bool IntersectAxis(double origin, double direction, double min, double max, ref double tMin, ref double tMax)
    {
        const double epsilon = 1e-10;
        if (Math.Abs(direction) < epsilon)
        {
            return origin >= min && origin <= max;
        }

        var t1 = (min - origin) / direction;
        var t2 = (max - origin) / direction;
        if (t1 > t2)
        {
            (t1, t2) = (t2, t1);
        }

        tMin = Math.Max(tMin, t1);
        tMax = Math.Min(tMax, t2);
        return tMin <= tMax;
    }
}
