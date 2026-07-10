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

    [Fact]
    public void Build_NormalsCountMatchesPositionsCount_ForSharedVertexMesh()
    {
        // WPF's MeshGeometry3D lights incorrectly when Positions and Normals have
        // different counts. The builder must ensure a 1:1 ratio even for meshes
        // that share vertex positions across faces with different normals.
        var mesh = CreateCubeLikeMesh();

        var geometry = MeshGeometryBuilder.Build(mesh);

        Assert.Equal(geometry.Positions.Count, geometry.Normals.Count);
    }

    [Fact]
    public void Build_KeepsCorrectTriangleCount_ForSharedVertexMesh()
    {
        var mesh = CreateCubeLikeMesh();

        var geometry = MeshGeometryBuilder.Build(mesh);

        Assert.Equal(12 * 3, geometry.TriangleIndices.Count);
        Assert.All(geometry.TriangleIndices, idx =>
        {
            Assert.InRange(idx, 0, geometry.Positions.Count - 1);
        });
    }

    [Fact]
    public void Build_ComputesFaceNormals_WhenObjDoesNotProvideNormals()
    {
        var mesh = new Mesh();
        mesh.AddVertex(new Vertex(0, 0, 0));
        mesh.AddVertex(new Vertex(1, 0, 0));
        mesh.AddVertex(new Vertex(0, 1, 0));
        mesh.AddVertex(new Vertex(0, 0, 1));
        mesh.Faces.Add(new Face(
            new FaceVertex(0, null, null),
            new FaceVertex(1, null, null),
            new FaceVertex(2, null, null)));
        mesh.Faces.Add(new Face(
            new FaceVertex(0, null, null),
            new FaceVertex(3, null, null),
            new FaceVertex(1, null, null)));

        var geometry = MeshGeometryBuilder.Build(mesh);

        Assert.Equal(6, geometry.Positions.Count);
        Assert.Equal(6, geometry.Normals.Count);
        Assert.All(geometry.Normals.Take(3), normal => Assert.Equal(1, normal.Z, 3));
        Assert.All(geometry.Normals.Skip(3), normal => Assert.Equal(1, normal.Y, 3));
    }

    private static Mesh CreateCubeLikeMesh()
    {
        var mesh = new Mesh();
        mesh.AddVertex(new Vertex(-1, -1, -1));   // 0
        mesh.AddVertex(new Vertex(1, -1, -1));    // 1
        mesh.AddVertex(new Vertex(1, 1, -1));     // 2
        mesh.AddVertex(new Vertex(-1, 1, -1));    // 3
        mesh.AddVertex(new Vertex(-1, -1, 1));    // 4
        mesh.AddVertex(new Vertex(1, -1, 1));     // 5
        mesh.AddVertex(new Vertex(1, 1, 1));      // 6
        mesh.AddVertex(new Vertex(-1, 1, 1));     // 7
        mesh.Normals.Add(new Normal(0, 0, -1));   // 0
        mesh.Normals.Add(new Normal(0, 0, 1));    // 1
        mesh.Normals.Add(new Normal(0, -1, 0));   // 2
        mesh.Normals.Add(new Normal(0, 1, 0));    // 3
        mesh.Normals.Add(new Normal(-1, 0, 0));   // 4
        mesh.Normals.Add(new Normal(1, 0, 0));    // 5
        mesh.Faces.Add(new Face(new FaceVertex(0, null, 0), new FaceVertex(3, null, 0), new FaceVertex(2, null, 0)));
        mesh.Faces.Add(new Face(new FaceVertex(0, null, 0), new FaceVertex(2, null, 0), new FaceVertex(1, null, 0)));
        mesh.Faces.Add(new Face(new FaceVertex(4, null, 1), new FaceVertex(5, null, 1), new FaceVertex(6, null, 1)));
        mesh.Faces.Add(new Face(new FaceVertex(4, null, 1), new FaceVertex(6, null, 1), new FaceVertex(7, null, 1)));
        mesh.Faces.Add(new Face(new FaceVertex(0, null, 2), new FaceVertex(1, null, 2), new FaceVertex(5, null, 2)));
        mesh.Faces.Add(new Face(new FaceVertex(0, null, 2), new FaceVertex(5, null, 2), new FaceVertex(4, null, 2)));
        mesh.Faces.Add(new Face(new FaceVertex(3, null, 3), new FaceVertex(7, null, 3), new FaceVertex(6, null, 3)));
        mesh.Faces.Add(new Face(new FaceVertex(3, null, 3), new FaceVertex(6, null, 3), new FaceVertex(2, null, 3)));
        mesh.Faces.Add(new Face(new FaceVertex(0, null, 4), new FaceVertex(4, null, 4), new FaceVertex(7, null, 4)));
        mesh.Faces.Add(new Face(new FaceVertex(0, null, 4), new FaceVertex(7, null, 4), new FaceVertex(3, null, 4)));
        mesh.Faces.Add(new Face(new FaceVertex(1, null, 5), new FaceVertex(2, null, 5), new FaceVertex(6, null, 5)));
        mesh.Faces.Add(new Face(new FaceVertex(1, null, 5), new FaceVertex(6, null, 5), new FaceVertex(5, null, 5)));
        return mesh;
    }
}
