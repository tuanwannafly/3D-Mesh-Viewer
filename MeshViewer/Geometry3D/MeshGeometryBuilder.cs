using System.Windows.Media.Media3D;
using MeshViewer.Models;

namespace MeshViewer.Geometry3D;

public static class MeshGeometryBuilder
{
    public static MeshGeometry3D Build(Mesh mesh, int? excludedFaceIndex = null)
    {
        ArgumentNullException.ThrowIfNull(mesh);

        var geometry = new MeshGeometry3D();

        foreach (var vertex in mesh.Vertices)
        {
            geometry.Positions.Add(new Point3D(vertex.X, vertex.Y, vertex.Z));
        }

        for (var i = 0; i < mesh.Faces.Count; i++)
        {
            if (i == excludedFaceIndex)
            {
                continue;
            }

            AddFace(geometry, mesh, mesh.Faces[i]);
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
        AddFaceVertex(geometry, mesh, mesh.Faces[faceIndex].A);
        AddFaceVertex(geometry, mesh, mesh.Faces[faceIndex].B);
        AddFaceVertex(geometry, mesh, mesh.Faces[faceIndex].C);
        geometry.TriangleIndices.Add(0);
        geometry.TriangleIndices.Add(1);
        geometry.TriangleIndices.Add(2);

        return geometry;
    }

    private static void AddFaceVertex(MeshGeometry3D geometry, Mesh mesh, FaceVertex faceVertex)
    {
        if (faceVertex.VertexIndex < 0 || faceVertex.VertexIndex >= mesh.Vertices.Count)
        {
            throw new InvalidOperationException($"Face references missing vertex index {faceVertex.VertexIndex}.");
        }

        var vertex = mesh.Vertices[faceVertex.VertexIndex];
        geometry.Positions.Add(new Point3D(vertex.X, vertex.Y, vertex.Z));
        AddNormalIfPresent(geometry, mesh, faceVertex.NormalIndex);
    }

    private static void AddFace(MeshGeometry3D geometry, Mesh mesh, Face face)
    {
        AddTriangleIndex(geometry, face.A.VertexIndex);
        AddTriangleIndex(geometry, face.B.VertexIndex);
        AddTriangleIndex(geometry, face.C.VertexIndex);

        AddNormalIfPresent(geometry, mesh, face.A.NormalIndex);
        AddNormalIfPresent(geometry, mesh, face.B.NormalIndex);
        AddNormalIfPresent(geometry, mesh, face.C.NormalIndex);
    }

    private static void AddTriangleIndex(MeshGeometry3D geometry, int index)
    {
        if (index < 0 || index >= geometry.Positions.Count)
        {
            throw new InvalidOperationException($"Face references missing vertex index {index}.");
        }

        geometry.TriangleIndices.Add(index);
    }

    private static void AddNormalIfPresent(MeshGeometry3D geometry, Mesh mesh, int? normalIndex)
    {
        if (normalIndex is null)
        {
            return;
        }

        if (normalIndex < 0 || normalIndex >= mesh.Normals.Count)
        {
            throw new InvalidOperationException($"Face references missing normal index {normalIndex}.");
        }

        var normal = mesh.Normals[normalIndex.Value];
        geometry.Normals.Add(new Vector3D(normal.X, normal.Y, normal.Z));
    }
}
