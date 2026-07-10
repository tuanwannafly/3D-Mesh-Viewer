using System.Windows.Media.Media3D;

namespace MeshViewer.Geometry3D;

public static class RayTriangleIntersection
{
    public static double? Intersect(Ray3D ray, Point3D a, Point3D b, Point3D c)
    {
        const double epsilon = 1e-10;
        var normalizedRay = ray.Normalize();
        var edge1 = b - a;
        var edge2 = c - a;
        var h = Vector3D.CrossProduct(normalizedRay.Direction, edge2);
        var determinant = Vector3D.DotProduct(edge1, h);

        if (Math.Abs(determinant) < epsilon)
        {
            return null;
        }

        var inverseDeterminant = 1 / determinant;
        var s = normalizedRay.Origin - a;
        var u = inverseDeterminant * Vector3D.DotProduct(s, h);
        if (u < -epsilon || u > 1 + epsilon)
        {
            return null;
        }

        var q = Vector3D.CrossProduct(s, edge1);
        var v = inverseDeterminant * Vector3D.DotProduct(normalizedRay.Direction, q);
        if (v < -epsilon || u + v > 1 + epsilon)
        {
            return null;
        }

        var t = inverseDeterminant * Vector3D.DotProduct(edge2, q);
        return t >= 0 ? t : null;
    }
}
