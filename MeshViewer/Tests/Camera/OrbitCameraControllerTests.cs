using System.Windows.Media.Media3D;
using MeshViewer.Camera;
using MeshViewer.Models;

namespace MeshViewer.Tests.Camera;

public sealed class OrbitCameraControllerTests
{
    [Fact]
    public void FitToView_TargetsBoundingBoxCenter()
    {
        var camera = new PerspectiveCamera { FieldOfView = 45 };
        var controller = new OrbitCameraController(camera);
        var bounds = new BoundingBox(new Vertex(-2, -4, -6), new Vertex(2, 4, 6));

        controller.FitToView(bounds, 16d / 9d);

        Assert.Equal(new Point3D(0, 0, 0), controller.Target);
        Assert.True(controller.Distance > 0);
        Assert.Equal(controller.Target - camera.Position, camera.LookDirection);
    }

    [Fact]
    public void Rotate_ClampsPitchToAvoidGimbalLock()
    {
        var controller = new OrbitCameraController(new PerspectiveCamera { FieldOfView = 45 });

        controller.Rotate(0, -1_000);

        Assert.InRange(controller.PitchDegrees, 88.9, 89);
    }

    [Fact]
    public void Zoom_ClampsDistanceInsideFitBounds()
    {
        var controller = new OrbitCameraController(new PerspectiveCamera { FieldOfView = 45 });
        controller.FitToView(new BoundingBox(new Vertex(-1, -1, -1), new Vertex(1, 1, 1)), 1);

        for (var i = 0; i < 100; i++)
        {
            controller.Zoom(120);
        }

        Assert.Equal(controller.MinDistance, controller.Distance);

        for (var i = 0; i < 200; i++)
        {
            controller.Zoom(-120);
        }

        Assert.Equal(controller.MaxDistance, controller.Distance);
    }

    [Fact]
    public void FitToView_ResetsOrbitAnglesToZero()
    {
        var controller = new OrbitCameraController(new PerspectiveCamera { FieldOfView = 45 });
        var bounds = new BoundingBox(new Vertex(-1, -1, -1), new Vertex(1, 1, 1));

        controller.Rotate(500, 500);
        Assert.NotEqual(0, controller.YawDegrees);
        Assert.NotEqual(0, controller.PitchDegrees);

        controller.FitToView(bounds, 1);

        Assert.Equal(0, controller.YawDegrees);
        Assert.Equal(0, controller.PitchDegrees);
    }

    [Fact]
    public void FitToViewPreservingOrientation_KeepsOrbitAngles()
    {
        var controller = new OrbitCameraController(new PerspectiveCamera { FieldOfView = 45 });
        var bounds = new BoundingBox(new Vertex(-1, -1, -1), new Vertex(1, 1, 1));

        controller.Rotate(40, 20);
        var yawBefore = controller.YawDegrees;
        var pitchBefore = controller.PitchDegrees;

        controller.FitToViewPreservingOrientation(bounds, 1);

        Assert.Equal(yawBefore, controller.YawDegrees);
        Assert.Equal(pitchBefore, controller.PitchDegrees);
    }
}
