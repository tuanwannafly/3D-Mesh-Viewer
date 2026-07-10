using System.Windows.Media.Media3D;
using MeshViewer.Geometry3D;

namespace MeshViewer.Tests.Geometry3D;

public sealed class RayTriangleIntersectionTests
{
    private static readonly Point3D A = new(0, 0, 0);
    private static readonly Point3D B = new(1, 0, 0);
    private static readonly Point3D C = new(0, 1, 0);

    [Fact]
    public void Intersect_ReturnsDistanceWhenRayHitsTriangleCenter()
    {
        var ray = new Ray3D(new Point3D(0.25, 0.25, 1), new Vector3D(0, 0, -1));

        var t = RayTriangleIntersection.Intersect(ray, A, B, C);

        Assert.Equal(1, t);
    }

    [Fact]
    public void Intersect_ReturnsNullWhenRayIsParallel()
    {
        var ray = new Ray3D(new Point3D(0.25, 0.25, 1), new Vector3D(1, 0, 0));

        Assert.Null(RayTriangleIntersection.Intersect(ray, A, B, C));
    }

    [Fact]
    public void Intersect_ReturnsDistanceForEdgeOrVertexHit()
    {
        var edgeRay = new Ray3D(new Point3D(0.5, 0, 1), new Vector3D(0, 0, -1));
        var vertexRay = new Ray3D(new Point3D(0, 0, 1), new Vector3D(0, 0, -1));

        Assert.Equal(1, RayTriangleIntersection.Intersect(edgeRay, A, B, C));
        Assert.Equal(1, RayTriangleIntersection.Intersect(vertexRay, A, B, C));
    }

    [Fact]
    public void Intersect_ReturnsNullWhenIntersectionIsOutsideTriangleOrBehindCamera()
    {
        var outsideRay = new Ray3D(new Point3D(2, 2, 1), new Vector3D(0, 0, -1));
        var behindRay = new Ray3D(new Point3D(0.25, 0.25, 1), new Vector3D(0, 0, 1));

        Assert.Null(RayTriangleIntersection.Intersect(outsideRay, A, B, C));
        Assert.Null(RayTriangleIntersection.Intersect(behindRay, A, B, C));
    }
}
