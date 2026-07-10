using System.Windows;
using System.Windows.Media.Media3D;
using MeshViewer.Models;

namespace MeshViewer.Camera;

public sealed class OrbitCameraController
{
    private const double MinPitchDegrees = -89;
    private const double MaxPitchDegrees = 89;

    private readonly PerspectiveCamera camera;

    public OrbitCameraController(PerspectiveCamera camera)
    {
        this.camera = camera;
        MinDistance = 0.1;
        MaxDistance = 10_000;
        Distance = 5;
        Target = new Point3D();
        YawDegrees = 0;
        PitchDegrees = 0;
        UpdateCamera();
    }

    public Point3D Target { get; private set; }

    public double Distance { get; private set; }

    public double MinDistance { get; private set; }

    public double MaxDistance { get; private set; }

    public double YawDegrees { get; private set; }

    public double PitchDegrees { get; private set; }

    public void Rotate(double deltaX, double deltaY)
    {
        YawDegrees += deltaX * 0.35;
        PitchDegrees = Math.Clamp(PitchDegrees - deltaY * 0.35, MinPitchDegrees, MaxPitchDegrees);
        UpdateCamera();
    }

    public void Zoom(double wheelDelta)
    {
        var scale = wheelDelta > 0 ? 0.9 : 1.1;
        Distance = Math.Clamp(Distance * scale, MinDistance, MaxDistance);
        UpdateCamera();
    }

    public void Pan(double deltaX, double deltaY, double viewportHeight)
    {
        if (viewportHeight <= 0)
        {
            return;
        }

        var lookDirection = camera.LookDirection;
        lookDirection.Normalize();

        var right = Vector3D.CrossProduct(lookDirection, camera.UpDirection);
        right.Normalize();

        var up = camera.UpDirection;
        up.Normalize();

        var worldUnitsPerPixel = 2 * Distance * Math.Tan(DegreesToRadians(camera.FieldOfView / 2)) / viewportHeight;
        Target += (-right * deltaX + up * deltaY) * worldUnitsPerPixel;
        UpdateCamera();
    }

    public void FitToView(BoundingBox bounds, double aspectRatio)
    {
        Target = ToPoint(bounds.Center);

        var maxDimension = Math.Max(bounds.MaxDimension, 1);
        var verticalFovRadians = DegreesToRadians(camera.FieldOfView);
        var horizontalFovRadians = 2 * Math.Atan(Math.Tan(verticalFovRadians / 2) * Math.Max(aspectRatio, 0.1));
        var fitVertical = maxDimension / (2 * Math.Tan(verticalFovRadians / 2));
        var fitHorizontal = maxDimension / (2 * Math.Tan(horizontalFovRadians / 2));

        Distance = Math.Max(fitVertical, fitHorizontal) * 1.8;
        MinDistance = Math.Max(maxDimension * 0.05, 0.1);
        MaxDistance = Math.Max(maxDimension * 20, MinDistance * 2);
        Distance = Math.Clamp(Distance, MinDistance, MaxDistance);
        UpdateCamera();
    }

    private void UpdateCamera()
    {
        var yaw = DegreesToRadians(YawDegrees);
        var pitch = DegreesToRadians(PitchDegrees);
        var cosPitch = Math.Cos(pitch);
        var offset = new Vector3D(
            Distance * cosPitch * Math.Sin(yaw),
            Distance * Math.Sin(pitch),
            Distance * cosPitch * Math.Cos(yaw));

        camera.Position = Target + offset;
        camera.LookDirection = Target - camera.Position;
        camera.UpDirection = new Vector3D(0, 1, 0);
    }

    private static Point3D ToPoint(Vertex vertex) => new(vertex.X, vertex.Y, vertex.Z);

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180;
}
