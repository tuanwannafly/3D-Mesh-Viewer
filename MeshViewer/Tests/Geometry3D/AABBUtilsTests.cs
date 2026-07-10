using System.Windows.Media.Media3D;
using MeshViewer.Geometry3D;
using MeshViewer.Models;

namespace MeshViewer.Tests.Geometry3D;

public sealed class AABBUtilsTests
{
    private static readonly BoundingBox Bounds = new(new Vertex(-1, -1, -1), new Vertex(1, 1, 1));

    [Fact]
    public void Intersects_ReturnsTrueWhenRayHitsBox()
    {
        var ray = new Ray3D(new Point3D(0, 0, 5), new Vector3D(0, 0, -1));

        Assert.True(AABBUtils.Intersects(ray, Bounds));
    }

    [Fact]
    public void Intersects_ReturnsFalseWhenRayMissesBox()
    {
        var ray = new Ray3D(new Point3D(3, 0, 5), new Vector3D(0, 0, -1));

        Assert.False(AABBUtils.Intersects(ray, Bounds));
    }
}
