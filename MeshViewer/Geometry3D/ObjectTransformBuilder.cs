using System.Windows.Media.Media3D;

namespace MeshViewer.Geometry3D;

public static class ObjectTransformBuilder
{
    public static Transform3DGroup Build(ObjectTransform transform)
    {
        var group = new Transform3DGroup();
        group.Children.Add(new ScaleTransform3D(transform.ScaleX, transform.ScaleY, transform.ScaleZ));
        group.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), transform.RotateX)));
        group.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), transform.RotateY)));
        group.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), transform.RotateZ)));
        group.Children.Add(new TranslateTransform3D(transform.TranslateX, transform.TranslateY, transform.TranslateZ));

        return group;
    }
}
