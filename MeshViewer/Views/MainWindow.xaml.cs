using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using MeshViewer.Camera;
using MeshViewer.Geometry3D;
using MeshViewer.Models;

namespace MeshViewer.Views;

public partial class MainWindow : Window
{
    private readonly OrbitCameraController cameraController;
    private Mesh? currentMesh;
    private Model3DGroup? sceneLights;
    private ModelVisual3D? meshVisual;
    private ModelVisual3D? selectedFaceVisual;
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
        sceneLights = new Model3DGroup();
        sceneLights.Children.Add(new AmbientLight(Color.FromRgb(70, 70, 70)));
        sceneLights.Children.Add(new DirectionalLight(Colors.White, new Vector3D(-1, -2, -3)));

        RenderMeshVisual(null);

        MainViewport.Children.Clear();
        MainViewport.Children.Add(new ModelVisual3D { Content = sceneLights });
        AddMeshVisualsToViewport();
        ResetView();
    }

    private void RenderMeshVisual(int? selectedFaceIndex)
    {
        if (currentMesh is null)
        {
            return;
        }

        var meshMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(86, 156, 214)));
        var meshModel = new GeometryModel3D(MeshGeometryBuilder.Build(currentMesh, selectedFaceIndex), meshMaterial)
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
        RenderMeshVisual(result?.FaceIndex);

        if (sceneLights is not null)
        {
            MainViewport.Children.Clear();
            MainViewport.Children.Add(new ModelVisual3D { Content = sceneLights });
            AddMeshVisualsToViewport();
        }
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
