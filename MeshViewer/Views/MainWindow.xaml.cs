using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Microsoft.Win32;
using MeshViewer.Camera;
using MeshViewer.Geometry3D;
using MeshViewer.Models;
using MeshViewer.Parsing;

namespace MeshViewer.Views;

public partial class MainWindow : Window
{
    private readonly OrbitCameraController cameraController;
    private Mesh? currentMesh;
    private Model3DGroup? sceneLights;
    private ModelVisual3D? meshVisual;
    private ModelVisual3D? selectedFaceVisual;
    private int? selectedFaceIndex;
    private Point lastMousePosition;
    private bool isOrbiting;
    private bool isPanning;
    private bool mouseMovedDuringDrag;

    public MainWindow()
    {
        InitializeComponent();
        cameraController = new OrbitCameraController(SceneCamera);
        RenderMesh(CreatePreviewMesh());
    }

    private void RenderMesh(Mesh mesh)
    {
        currentMesh = mesh;
        selectedFaceIndex = null;
        sceneLights = new Model3DGroup();
        sceneLights.Children.Add(new AmbientLight(Color.FromRgb(70, 70, 70)));
        sceneLights.Children.Add(new DirectionalLight(Colors.White, new Vector3D(-1, -2, -3)));

        RenderMeshVisual(null);

        MainViewport.Children.Clear();
        MainViewport.Children.Add(new ModelVisual3D { Content = sceneLights });
        AddMeshVisualsToViewport();
        UpdateMeshStats();
        ResetView();
    }

    private void RenderMeshVisual(int? selectedFaceIndex)
    {
        if (currentMesh is null)
        {
            return;
        }

        var isWireframe = WireframeToggle.IsChecked == true;
        var meshMaterial = new DiffuseMaterial(new SolidColorBrush(isWireframe ? Colors.Black : Color.FromRgb(86, 156, 214)));
        var geometry = isWireframe
            ? WireframeGeometryBuilder.Build(currentMesh, Math.Max(currentMesh.BoundingBox?.MaxDimension ?? 1, 1) * 0.003)
            : MeshGeometryBuilder.Build(currentMesh, selectedFaceIndex);
        var meshModel = new GeometryModel3D(geometry, meshMaterial)
        {
            BackMaterial = meshMaterial
        };

        meshVisual = new ModelVisual3D { Content = meshModel };

        selectedFaceVisual = null;
        if (selectedFaceIndex is not null)
        {
            var selectedMaterial = new DiffuseMaterial(Brushes.Red);
            selectedFaceVisual = new ModelVisual3D
            {
                Content = new GeometryModel3D(MeshGeometryBuilder.BuildFace(currentMesh, selectedFaceIndex.Value), selectedMaterial)
                {
                    BackMaterial = selectedMaterial
                }
            };
        }

        ApplyMeshTransform();
    }

    private void AddMeshVisualsToViewport()
    {
        if (meshVisual is not null)
        {
            MainViewport.Children.Add(meshVisual);
        }

        if (selectedFaceVisual is not null)
        {
            MainViewport.Children.Add(selectedFaceVisual);
        }
    }

    private void TransformSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => ApplyMeshTransform();

    private void LoadObjButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "OBJ files (*.obj)|*.obj|All files (*.*)|*.*",
            Title = "Load OBJ"
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        var mesh = new ObjParser().Parse(File.ReadAllText(dialog.FileName));
        RenderMesh(mesh);
    }

    private void RenderModeToggle_Changed(object sender, RoutedEventArgs e)
    {
        if (currentMesh is null)
        {
            return;
        }

        RebuildScene();
    }

    private void ApplyMeshTransform()
    {
        if (meshVisual is null)
        {
            return;
        }

        var transform = ObjectTransformBuilder.Build(new ObjectTransform(
            RotateXSlider.Value,
            RotateYSlider.Value,
            RotateZSlider.Value,
            TranslateXSlider.Value,
            TranslateYSlider.Value,
            TranslateZSlider.Value,
            ScaleXSlider.Value,
            ScaleYSlider.Value,
            ScaleZSlider.Value));

        meshVisual.Transform = transform;
        if (selectedFaceVisual is not null)
        {
            selectedFaceVisual.Transform = transform;
        }
    }

    private void ResetView()
    {
        if (currentMesh?.BoundingBox is not { } bounds)
        {
            return;
        }

        var aspectRatio = MainViewport.ActualHeight > 0
            ? MainViewport.ActualWidth / MainViewport.ActualHeight
            : 1;

        cameraController.FitToView(bounds, aspectRatio);
    }

    private void ResetViewButton_Click(object sender, RoutedEventArgs e) => ResetView();

    private void MainViewport_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        MainViewport.Focus();
        lastMousePosition = e.GetPosition(MainViewport);
        mouseMovedDuringDrag = false;

        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
        {
            isPanning = true;
        }
        else
        {
            isOrbiting = true;
        }

        MainViewport.CaptureMouse();
    }

    private void MainViewport_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (isOrbiting && !mouseMovedDuringDrag)
        {
            PickFace(e.GetPosition(MainViewport));
        }

        StopMouseDrag();
    }

    private void MainViewport_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Middle)
        {
            return;
        }

        MainViewport.Focus();
        lastMousePosition = e.GetPosition(MainViewport);
        mouseMovedDuringDrag = false;
        isPanning = true;
        MainViewport.CaptureMouse();
    }

    private void MainViewport_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Middle)
        {
            StopMouseDrag();
        }
    }

    private void MainViewport_MouseMove(object sender, MouseEventArgs e)
    {
        if (!isOrbiting && !isPanning)
        {
            return;
        }

        var currentPosition = e.GetPosition(MainViewport);
        var delta = currentPosition - lastMousePosition;
        mouseMovedDuringDrag |= Math.Abs(delta.X) > 2 || Math.Abs(delta.Y) > 2;
        lastMousePosition = currentPosition;

        if (isPanning)
        {
            cameraController.Pan(delta.X, delta.Y, MainViewport.ActualHeight);
        }
        else
        {
            cameraController.Rotate(delta.X, delta.Y);
        }
    }

    private void MainViewport_MouseWheel(object sender, MouseWheelEventArgs e) => cameraController.Zoom(e.Delta);

    private void PickFace(Point position)
    {
        if (currentMesh is null || meshVisual is null || MainViewport.ActualWidth <= 0 || MainViewport.ActualHeight <= 0)
        {
            return;
        }

        var ray = ScreenRayCaster.CreateRay(position, SceneCamera, MainViewport.ActualWidth, MainViewport.ActualHeight);
        var result = MeshRayPicker.Pick(currentMesh, ray, meshVisual.Transform.Value);
        selectedFaceIndex = result?.FaceIndex;
        RebuildScene();
    }

    private void RebuildScene()
    {
        RenderMeshVisual(selectedFaceIndex);
        if (sceneLights is not null)
        {
            MainViewport.Children.Clear();
            MainViewport.Children.Add(new ModelVisual3D { Content = sceneLights });
            AddMeshVisualsToViewport();
        }
    }

    private void UpdateMeshStats()
    {
        MeshStatsText.Text = currentMesh is null
            ? "No mesh loaded"
            : $"Vertices: {currentMesh.Vertices.Count} | Faces: {currentMesh.Faces.Count}";
    }

    private void StopMouseDrag()
    {
        isOrbiting = false;
        isPanning = false;
        MainViewport.ReleaseMouseCapture();
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
