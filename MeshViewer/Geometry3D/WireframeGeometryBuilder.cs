using System.Windows.Media.Media3D;
using MeshViewer.Models;

namespace MeshViewer.Geometry3D;

public static class WireframeGeometryBuilder
{
    public static MeshGeometry3D Build(Mesh mesh, double thickness)
    {
        ArgumentNullException.ThrowIfNull(mesh);

        var geometry = new MeshGeometry3D();
        var edges = new HashSet<(int A, int B)>();

        foreach (var face in mesh.Faces)
        {
            AddEdge(edges, face.A.VertexIndex, face.B.VertexIndex);
            AddEdge(edges, face.B.VertexIndex, face.C.VertexIndex);
            AddEdge(edges, face.C.VertexIndex, face.A.VertexIndex);
        }

        foreach (var (a, b) in edges)
        {
            AddEdgePrism(geometry, ToPoint(mesh.Vertices[a]), ToPoint(mesh.Vertices[b]), thickness);
        }

        return geometry;
    }

    private static void AddEdge(HashSet<(int A, int B)> edges, int a, int b)
    {
        edges.Add(a < b ? (a, b) : (b, a));
    }

    private static void AddEdgePrism(MeshGeometry3D geometry, Point3D start, Point3D end, double thickness)
    {
        var direction = end - start;
        if (direction.LengthSquared == 0)
        {
            return;
        }

        direction.Normalize();
        var reference = Math.Abs(Vector3D.DotProduct(direction, new Vector3D(0, 1, 0))) > 0.9
            ? new Vector3D(1, 0, 0)
            : new Vector3D(0, 1, 0);
        var right = Vector3D.CrossProduct(direction, reference);
        right.Normalize();
        right *= thickness;
        var up = Vector3D.CrossProduct(direction, right);
        up.Normalize();
        up *= thickness;

        var baseIndex = geometry.Positions.Count;
        geometry.Positions.Add(start - right - up);
        geometry.Positions.Add(start + right - up);
        geometry.Positions.Add(start + right + up);
        geometry.Positions.Add(start - right + up);
        geometry.Positions.Add(end - right - up);
        geometry.Positions.Add(end + right - up);
        geometry.Positions.Add(end + right + up);
        geometry.Positions.Add(end - right + up);

        AddQuad(geometry, baseIndex, 0, 1, 5, 4);
        AddQuad(geometry, baseIndex, 1, 2, 6, 5);
        AddQuad(geometry, baseIndex, 2, 3, 7, 6);
        AddQuad(geometry, baseIndex, 3, 0, 4, 7);
    }

    private static void AddQuad(MeshGeometry3D geometry, int baseIndex, int a, int b, int c, int d)
    {
        geometry.TriangleIndices.Add(baseIndex + a);
        geometry.TriangleIndices.Add(baseIndex + b);
        geometry.TriangleIndices.Add(baseIndex + c);
        geometry.TriangleIndices.Add(baseIndex + a);
        geometry.TriangleIndices.Add(baseIndex + c);
        geometry.TriangleIndices.Add(baseIndex + d);
    }

    private static Point3D ToPoint(Vertex vertex) => new(vertex.X, vertex.Y, vertex.Z);
}
