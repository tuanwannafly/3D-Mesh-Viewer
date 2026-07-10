using System.Collections.Generic;

namespace MeshViewer.Models;

public sealed class Mesh
{
    public List<Vertex> Vertices { get; } = [];

    public List<TextureCoordinate> TextureCoordinates { get; } = [];

    public List<Normal> Normals { get; } = [];

    public List<Face> Faces { get; } = [];

    public BoundingBox? BoundingBox { get; private set; }

    public void AddVertex(Vertex vertex)
    {
        Vertices.Add(vertex);
        BoundingBox = BoundingBox is null
            ? Models.BoundingBox.FromVertex(vertex)
            : BoundingBox.Value.Include(vertex);
    }
}
