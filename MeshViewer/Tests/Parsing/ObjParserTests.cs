using MeshViewer.Models;
using MeshViewer.Parsing;

namespace MeshViewer.Tests.Parsing;

public sealed class ObjParserTests
{
    [Fact]
    public void Parse_ReadsSupportedObjDataAndTriangulatesQuads()
    {
        const string obj = """
            # basic mesh
            o IgnoredObjectName
            v -1 0 2
            v 3 4 -2
            v 0 5 1
            v 2 -3 6
            vt 0 0
            vt 1 0
            vt 1 1
            vn 0 0 1
            vn 0 1 0
            f 1/1/1 2/2/1 3/3/2
            usemtl ignored-material
            f 1//1 3//2 4//1 2//2
            """;

        var mesh = new ObjParser().Parse(obj);

        Assert.Equal(4, mesh.Vertices.Count);
        Assert.Equal(3, mesh.TextureCoordinates.Count);
        Assert.Equal(2, mesh.Normals.Count);
        Assert.Equal(3, mesh.Faces.Count);
        Assert.Equal(new BoundingBox(new Vertex(-1, -3, -2), new Vertex(3, 5, 6)), mesh.BoundingBox);

        Assert.Equal(new FaceVertex(0, 0, 0), mesh.Faces[0].A);
        Assert.Equal(new FaceVertex(1, 1, 0), mesh.Faces[0].B);
        Assert.Equal(new FaceVertex(2, 2, 1), mesh.Faces[0].C);

        Assert.Equal(new FaceVertex(0, null, 0), mesh.Faces[1].A);
        Assert.Equal(new FaceVertex(2, null, 1), mesh.Faces[1].B);
        Assert.Equal(new FaceVertex(3, null, 0), mesh.Faces[1].C);

        Assert.Equal(new FaceVertex(0, null, 0), mesh.Faces[2].A);
        Assert.Equal(new FaceVertex(3, null, 0), mesh.Faces[2].B);
        Assert.Equal(new FaceVertex(1, null, 1), mesh.Faces[2].C);
    }
}
