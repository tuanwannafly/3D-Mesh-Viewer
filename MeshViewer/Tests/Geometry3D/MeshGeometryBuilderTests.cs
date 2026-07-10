using MeshViewer.Geometry3D;
using MeshViewer.Models;

namespace MeshViewer.Tests.Geometry3D;

public sealed class MeshGeometryBuilderTests
{
    [Fact]
    public void Build_ConvertsMeshToMeshGeometry3D()
    {
        var mesh = new Mesh();
        mesh.AddVertex(new Vertex(-1, 0, 0));
        mesh.AddVertex(new Vertex(1, 0, 0));
        mesh.AddVertex(new Vertex(0, 1, 0));
        mesh.Normals.Add(new Normal(0, 0, 1));
        mesh.Faces.Add(new Face(
            new FaceVertex(0, null, 0),
            new FaceVertex(1, null, 0),
            new FaceVertex(2, null, 0)));

        var geometry = MeshGeometryBuilder.Build(mesh);

        Assert.Equal(3, geometry.Positions.Count);
        Assert.Equal(-1, geometry.Positions[0].X);
        Assert.Equal(0, geometry.Positions[0].Y);
        Assert.Equal(0, geometry.Positions[0].Z);
        Assert.Equal([0, 1, 2], geometry.TriangleIndices);
        Assert.Equal(3, geometry.Normals.Count);
        Assert.All(geometry.Normals, normal => Assert.Equal(1, normal.Z));
    }
}
