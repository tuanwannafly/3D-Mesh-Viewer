using System.Windows;
using System.Windows.Media.Media3D;
using MeshViewer.Geometry3D;

namespace MeshViewer.Tests.Geometry3D;

public sealed class ScreenRayCasterTests
{
    [Fact]
    public void CreateRay_CenterPointMatchesCameraLookDirection()
    {
        var ray = ScreenRayCaster.CreateRay(
            new Point(400, 300),
            new Point3D(0, 0, 5),
            new Vector3D(0, 0, -1),
            new Vector3D(0, 1, 0),
            90,
            800,
            600);

        Assert.Equal(new Point3D(0, 0, 5), ray.Origin);
        AssertVectorClose(new Vector3D(0, 0, -1), ray.Direction);
    }

    [Fact]
    public void CreateRay_TopRightPointTiltsRightAndUp()
    {
        var ray = ScreenRayCaster.CreateRay(
            new Point(800, 0),
            new Point3D(0, 0, 0),
            new Vector3D(0, 0, -1),
            new Vector3D(0, 1, 0),
            90,
            800,
            400);

        Assert.True(ray.Direction.X > 0);
        Assert.True(ray.Direction.Y > 0);
        Assert.True(ray.Direction.Z < 0);
        Assert.InRange(ray.Direction.Length, 0.999999, 1.000001);
    }

    private static void AssertVectorClose(Vector3D expected, Vector3D actual)
    {
        Assert.Equal(expected.X, actual.X, 6);
        Assert.Equal(expected.Y, actual.Y, 6);
        Assert.Equal(expected.Z, actual.Z, 6);
    }
}
