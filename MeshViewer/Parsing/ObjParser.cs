using System.Globalization;
using System.IO;
using System.Linq;
using MeshViewer.Models;

namespace MeshViewer.Parsing;

public sealed class ObjParser
{
    public Mesh Parse(string content)
    {
        ArgumentNullException.ThrowIfNull(content);

        using var reader = new StringReader(content);
        return Parse(reader);
    }

    public Mesh Parse(TextReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        var mesh = new Mesh();
        string? line;

        while ((line = reader.ReadLine()) is not null)
        {
            ParseLine(line, mesh);
        }

        return mesh;
    }

    private static void ParseLine(string line, Mesh mesh)
    {
        var trimmed = line.Trim();
        if (trimmed.Length == 0 || trimmed.StartsWith('#'))
        {
            return;
        }

        var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0)
        {
            return;
        }

        switch (parts[0])
        {
            case "v":
                ParseVertex(parts, mesh);
                break;
            case "vt":
                ParseTextureCoordinate(parts, mesh);
                break;
            case "vn":
                ParseNormal(parts, mesh);
                break;
            case "f":
                ParseFace(parts, mesh);
                break;
        }
    }

    private static void ParseVertex(string[] parts, Mesh mesh)
    {
        if (parts.Length < 4)
        {
            return;
        }

        mesh.AddVertex(new Vertex(ParseDouble(parts[1]), ParseDouble(parts[2]), ParseDouble(parts[3])));
    }

    private static void ParseTextureCoordinate(string[] parts, Mesh mesh)
    {
        if (parts.Length < 3)
        {
            return;
        }

        mesh.TextureCoordinates.Add(new TextureCoordinate(ParseDouble(parts[1]), ParseDouble(parts[2])));
    }

    private static void ParseNormal(string[] parts, Mesh mesh)
    {
        if (parts.Length < 4)
        {
            return;
        }

        mesh.Normals.Add(new Normal(ParseDouble(parts[1]), ParseDouble(parts[2]), ParseDouble(parts[3])));
    }

    private static void ParseFace(string[] parts, Mesh mesh)
    {
        if (parts.Length < 4)
        {
            return;
        }

        var vertices = parts.Skip(1).Select(part => ParseFaceVertex(part, mesh)).ToArray();
        if (vertices.Length < 3)
        {
            return;
        }

        for (var i = 1; i < vertices.Length - 1; i++)
        {
            mesh.Faces.Add(new Face(vertices[0], vertices[i], vertices[i + 1]));
        }
    }

    private static FaceVertex ParseFaceVertex(string value, Mesh mesh)
    {
        var parts = value.Split('/');
        return new FaceVertex(
            ParseObjIndex(parts[0], mesh.Vertices.Count),
            parts.Length > 1 ? ParseOptionalObjIndex(parts[1], mesh.TextureCoordinates.Count) : null,
            parts.Length > 2 ? ParseOptionalObjIndex(parts[2], mesh.Normals.Count) : null);
    }

    private static int ParseObjIndex(string value, int itemCount)
    {
        var index = int.Parse(value, CultureInfo.InvariantCulture);
        return index < 0 ? itemCount + index : index - 1;
    }

    private static int? ParseOptionalObjIndex(string value, int itemCount)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : ParseObjIndex(value, itemCount);
    }

    private static double ParseDouble(string value) => double.Parse(value, CultureInfo.InvariantCulture);
}
