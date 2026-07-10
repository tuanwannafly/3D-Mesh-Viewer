using System.Windows;
using System.Windows.Media.Media3D;

namespace MeshViewer.Geometry3D;

public static class ScreenRayCaster
{
    public static Ray3D CreateRay(
        Point screenPoint,
        Point3D cameraPosition,
        Vector3D lookDirection,
        Vector3D upDirection,
        double fieldOfView,
        double viewportWidth,
        double viewportHeight)
    {
        if (viewportWidth <= 0 || viewportHeight <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(viewportWidth), "Viewport size must be positive.");
        }

        var forward = lookDirection;
        forward.Normalize();

        var right = Vector3D.CrossProduct(forward, upDirection);
        right.Normalize();

        var up = Vector3D.CrossProduct(right, forward);
        up.Normalize();

        var aspectRatio = viewportWidth / viewportHeight;
        var halfVertical = Math.Tan(DegreesToRadians(fieldOfView / 2));
        var halfHorizontal = halfVertical * aspectRatio;
        var normalizedX = (2 * screenPoint.X / viewportWidth) - 1;
        var normalizedY = 1 - (2 * screenPoint.Y / viewportHeight);

        var direction = forward
            + right * (normalizedX * halfHorizontal)
            + up * (normalizedY * halfVertical);
        direction.Normalize();

        return new Ray3D(cameraPosition, direction);
    }

    public static Ray3D CreateRay(Point screenPoint, PerspectiveCamera camera, double viewportWidth, double viewportHeight)
    {
        ArgumentNullException.ThrowIfNull(camera);

        return CreateRay(
            screenPoint,
            camera.Position,
            camera.LookDirection,
            camera.UpDirection,
            camera.FieldOfView,
            viewportWidth,
            viewportHeight);
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180;
}
