using System.Windows.Media.Media3D;

namespace MeshViewer.Geometry3D;

public readonly record struct Ray3D(Point3D Origin, Vector3D Direction)
{
    public Ray3D Normalize()
    {
        var direction = Direction;
        direction.Normalize();
        return new Ray3D(Origin, direction);
    }
}
