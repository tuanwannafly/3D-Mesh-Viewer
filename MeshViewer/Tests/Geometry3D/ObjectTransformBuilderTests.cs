using System.Windows.Media.Media3D;
using MeshViewer.Geometry3D;

namespace MeshViewer.Tests.Geometry3D;

public sealed class ObjectTransformBuilderTests
{
    [Fact]
    public void Build_CreatesScaleRotateAndTranslateTransforms()
    {
        var transform = new ObjectTransform(10, 20, 30, 1, 2, 3, 1.5, 2, 2.5);

        var group = ObjectTransformBuilder.Build(transform);

        Assert.IsType<ScaleTransform3D>(group.Children[0]);
        Assert.IsType<RotateTransform3D>(group.Children[1]);
        Assert.IsType<RotateTransform3D>(group.Children[2]);
        Assert.IsType<RotateTransform3D>(group.Children[3]);
        Assert.IsType<TranslateTransform3D>(group.Children[4]);

        var translation = Assert.IsType<TranslateTransform3D>(group.Children[4]);
        Assert.Equal(1, translation.OffsetX);
        Assert.Equal(2, translation.OffsetY);
        Assert.Equal(3, translation.OffsetZ);
    }
}
