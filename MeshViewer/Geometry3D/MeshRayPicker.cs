using System.Windows.Media.Media3D;
using MeshViewer.Models;

namespace MeshViewer.Geometry3D;

public static class MeshRayPicker
{
    public static MeshPickResult? Pick(Mesh mesh, Ray3D ray, Matrix3D transform = default)
    {
        ArgumentNullException.ThrowIfNull(mesh);

        if (mesh.Vertices.Count == 0 || mesh.Faces.Count == 0)
        {
            return null;
        }

        var matrix = transform.IsIdentity || transform == default ? Matrix3D.Identity : transform;
        var transformedVertices = mesh.Vertices
            .Select(vertex => matrix.Transform(new Point3D(vertex.X, vertex.Y, vertex.Z)))
            .ToArray();

        if (!AABBUtils.Intersects(ray, AABBUtils.FromPoints(transformedVertices)))
        {
            return null;
        }

        MeshPickResult? nearest = null;
        for (var i = 0; i < mesh.Faces.Count; i++)
        {
            var face = mesh.Faces[i];
            var t = RayTriangleIntersection.Intersect(
                ray,
                transformedVertices[face.A.VertexIndex],
                transformedVertices[face.B.VertexIndex],
                transformedVertices[face.C.VertexIndex]);

            if (t is null || (nearest is not null && t.Value >= nearest.Value.Distance))
            {
                continue;
            }

            nearest = new MeshPickResult(i, t.Value);
        }

        return nearest;
    }
}

public readonly record struct MeshPickResult(int FaceIndex, double Distance);
