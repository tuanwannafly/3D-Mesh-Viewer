using System.Windows.Media.Media3D;
using MeshViewer.Geometry3D;
using MeshViewer.Models;

namespace MeshViewer.Tests.Geometry3D;

public sealed class MeshRayPickerTests
{
    [Fact]
    public void Pick_ReturnsNearestIntersectedFace()
    {
        var mesh = new Mesh();
        mesh.AddVertex(new Vertex(0, 0, 0));
        mesh.AddVertex(new Vertex(1, 0, 0));
        mesh.AddVertex(new Vertex(0, 1, 0));
        mesh.AddVertex(new Vertex(0, 0, 2));
        mesh.AddVertex(new Vertex(1, 0, 2));
        mesh.AddVertex(new Vertex(0, 1, 2));
        mesh.Faces.Add(new Face(new FaceVertex(0, null, null), new FaceVertex(1, null, null), new FaceVertex(2, null, null)));
        mesh.Faces.Add(new Face(new FaceVertex(3, null, null), new FaceVertex(4, null, null), new FaceVertex(5, null, null)));

        var result = MeshRayPicker.Pick(mesh, new Ray3D(new Point3D(0.2, 0.2, 5), new Vector3D(0, 0, -1)));

        Assert.NotNull(result);
        Assert.Equal(1, result.Value.FaceIndex);
        Assert.Equal(3, result.Value.Distance);
    }
}
