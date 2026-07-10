namespace MeshViewer.Models;

public readonly record struct FaceVertex(int VertexIndex, int? TextureCoordinateIndex, int? NormalIndex);
