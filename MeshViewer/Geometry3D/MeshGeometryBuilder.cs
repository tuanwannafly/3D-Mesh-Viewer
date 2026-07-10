using System.Collections.Generic;
using System.Windows.Media.Media3D;
using MeshViewer.Models;

namespace MeshViewer.Geometry3D;

public static class MeshGeometryBuilder
{
    /// <summary>
    /// Builds a WPF <see cref="MeshGeometry3D"/> from a <see cref="Mesh"/>.
    ///
    /// WPF requires <see cref="MeshGeometry3D.Positions"/>, <see cref="MeshGeometry3D.Normals"/>,
    /// and <see cref="MeshGeometry3D.TextureCoordinates"/> to share indices — they all
    /// describe the same vertex slots. Plain shared-vertex meshes (one vertex used by many
    /// triangles with different normals) cannot be expressed as-is, so we deduplicate
    /// by <c>(vertex index, normal index)</c> and re-emit a unique slot for each combination.
    /// </summary>
    public static MeshGeometry3D Build(Mesh mesh, int? excludedFaceIndex = null)
    {
        ArgumentNullException.ThrowIfNull(mesh);

        var geometry = new MeshGeometry3D();
        var slotMap = new Dictionary<(int vertexIndex, int? normalIndex, int faceIndex), int>();

        for (var i = 0; i < mesh.Faces.Count; i++)
        {
            if (i == excludedFaceIndex)
            {
                continue;
            }

            AddFace(geometry, mesh, mesh.Faces[i], i, slotMap);
        }

        return geometry;
    }

    public static MeshGeometry3D BuildFace(Mesh mesh, int faceIndex)
    {
        ArgumentNullException.ThrowIfNull(mesh);

        if (faceIndex < 0 || faceIndex >= mesh.Faces.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(faceIndex));
        }

        var geometry = new MeshGeometry3D();
        var slotMap = new Dictionary<(int, int?, int), int>();
        var face = mesh.Faces[faceIndex];

        var fallbackNormal = CalculateFaceNormal(mesh, face);
        AppendVertexSlot(geometry, mesh, face.A.VertexIndex, face.A.NormalIndex, faceIndex, fallbackNormal, slotMap);
        AppendVertexSlot(geometry, mesh, face.B.VertexIndex, face.B.NormalIndex, faceIndex, fallbackNormal, slotMap);
        AppendVertexSlot(geometry, mesh, face.C.VertexIndex, face.C.NormalIndex, faceIndex, fallbackNormal, slotMap);
        geometry.TriangleIndices.Add(0);
        geometry.TriangleIndices.Add(1);
        geometry.TriangleIndices.Add(2);

        return geometry;
    }

    private static void AddFace(
        MeshGeometry3D geometry,
        Mesh mesh,
        Face face,
        int faceIndex,
        Dictionary<(int, int?, int), int> slotMap)
    {
        var fallbackNormal = CalculateFaceNormal(mesh, face);
        var a = AppendVertexSlot(geometry, mesh, face.A.VertexIndex, face.A.NormalIndex, faceIndex, fallbackNormal, slotMap);
        var b = AppendVertexSlot(geometry, mesh, face.B.VertexIndex, face.B.NormalIndex, faceIndex, fallbackNormal, slotMap);
        var c = AppendVertexSlot(geometry, mesh, face.C.VertexIndex, face.C.NormalIndex, faceIndex, fallbackNormal, slotMap);

        geometry.TriangleIndices.Add(a);
        geometry.TriangleIndices.Add(b);
        geometry.TriangleIndices.Add(c);
    }

    /// <summary>
    /// Appends a new (Position, Normal) slot for the requested vertex+normal pair, or returns
    /// the index of an existing matching slot. Guarantees <c>Positions.Count == Normals.Count</c>.
    /// </summary>
    private static int AppendVertexSlot(
        MeshGeometry3D geometry,
        Mesh mesh,
        int vertexIndex,
        int? normalIndex,
        int faceIndex,
        Vector3D fallbackNormal,
        Dictionary<(int, int?, int), int> slotMap)
    {
        if (vertexIndex < 0 || vertexIndex >= mesh.Vertices.Count)
        {
            throw new InvalidOperationException($"Face references missing vertex index {vertexIndex}.");
        }

        var key = (vertexIndex, normalIndex, normalIndex is null ? faceIndex : -1);
        if (slotMap.TryGetValue(key, out var existing))
        {
            return existing;
        }

        var vertex = mesh.Vertices[vertexIndex];
        var slot = geometry.Positions.Count;

        geometry.Positions.Add(new Point3D(vertex.X, vertex.Y, vertex.Z));

        if (normalIndex is null)
        {
            geometry.Normals.Add(fallbackNormal);
        }
        else
        {
            if (normalIndex < 0 || normalIndex >= mesh.Normals.Count)
            {
                throw new InvalidOperationException($"Face references missing normal index {normalIndex}.");
            }
            var normal = mesh.Normals[normalIndex.Value];
            geometry.Normals.Add(new Vector3D(normal.X, normal.Y, normal.Z));
        }

        slotMap[key] = slot;
        return slot;
    }

    private static Vector3D CalculateFaceNormal(Mesh mesh, Face face)
    {
        var a = ToPoint(mesh.Vertices[face.A.VertexIndex]);
        var b = ToPoint(mesh.Vertices[face.B.VertexIndex]);
        var c = ToPoint(mesh.Vertices[face.C.VertexIndex]);
        var normal = Vector3D.CrossProduct(b - a, c - a);
        if (normal.LengthSquared == 0)
        {
            return new Vector3D(0, 0, 1);
        }

        normal.Normalize();
        return normal;
    }

    private static Point3D ToPoint(Vertex vertex) => new(vertex.X, vertex.Y, vertex.Z);
}
