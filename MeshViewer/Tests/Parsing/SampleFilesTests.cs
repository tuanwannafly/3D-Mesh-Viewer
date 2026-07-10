using MeshViewer.Parsing;

namespace MeshViewer.Tests.Parsing;

public sealed class SampleFilesTests
{
    public static IEnumerable<object[]> SampleObjFiles()
    {
        foreach (var path in Directory.EnumerateFiles(GetSampleFilesDirectory(), "*.obj"))
        {
            yield return [path];
        }
    }

    [Theory]
    [MemberData(nameof(SampleObjFiles))]
    public void Parse_LoadsSampleObjFile(string path)
    {
        var mesh = new ObjParser().Parse(File.ReadAllText(path));

        Assert.NotEmpty(mesh.Vertices);
        Assert.NotEmpty(mesh.Faces);
        Assert.NotNull(mesh.BoundingBox);
    }

    [Fact]
    public void SampleFilesDirectory_ContainsAtLeastThreeObjFiles()
    {
        var files = Directory.EnumerateFiles(GetSampleFilesDirectory(), "*.obj");

        Assert.True(files.Count() >= 3);
    }

    private static string GetSampleFilesDirectory()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, "SampleFiles")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        return Path.Combine(directory.FullName, "SampleFiles");
    }
}
