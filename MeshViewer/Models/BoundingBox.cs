using System;

namespace MeshViewer.Models;

public readonly record struct BoundingBox(Vertex Min, Vertex Max)
{
    public static BoundingBox FromVertex(Vertex vertex) => new(vertex, vertex);

    public Vertex Center => new(
        (Min.X + Max.X) / 2,
        (Min.Y + Max.Y) / 2,
        (Min.Z + Max.Z) / 2);

    public double Width => Max.X - Min.X;

    public double Height => Max.Y - Min.Y;

    public double Depth => Max.Z - Min.Z;

    public double MaxDimension => Math.Max(Width, Math.Max(Height, Depth));

    public BoundingBox Include(Vertex vertex)
    {
        return new BoundingBox(
            new Vertex(
                Math.Min(Min.X, vertex.X),
                Math.Min(Min.Y, vertex.Y),
                Math.Min(Min.Z, vertex.Z)),
            new Vertex(
                Math.Max(Max.X, vertex.X),
                Math.Max(Max.Y, vertex.Y),
                Math.Max(Max.Z, vertex.Z)));
    }
}
