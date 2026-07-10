using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using MeshViewer.Geometry3D;
using MeshViewer.Models;

namespace MeshViewer.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        RenderMesh(CreatePreviewMesh());
    }

    private void RenderMesh(Mesh mesh)
    {
        var modelGroup = new Model3DGroup();
        modelGroup.Children.Add(new AmbientLight(Color.FromRgb(70, 70, 70)));
        modelGroup.Children.Add(new DirectionalLight(Colors.White, new Vector3D(-1, -2, -3)));

        var material = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(86, 156, 214)));
        modelGroup.Children.Add(new GeometryModel3D(MeshGeometryBuilder.Build(mesh), material)
        {
            BackMaterial = material
        });

        MainViewport.Children.Clear();
        MainViewport.Children.Add(new ModelVisual3D { Content = modelGroup });
    }

    private static Mesh CreatePreviewMesh()
    {
        var mesh = new Mesh();
        mesh.AddVertex(new Vertex(-1, -1, 0));
        mesh.AddVertex(new Vertex(1, -1, 0));
        mesh.AddVertex(new Vertex(0, 1, 0));
        mesh.Normals.Add(new Normal(0, 0, 1));
        mesh.Faces.Add(new Face(
            new FaceVertex(0, null, 0),
            new FaceVertex(1, null, 0),
            new FaceVertex(2, null, 0)));

        return mesh;
    }
}
