using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
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
    private readonly Dictionary<string, AxisRowControl> axisRows = new();
    private Mesh? currentMesh;
    private Model3DGroup? sceneLights;
    private ModelVisual3D? meshVisual;
    private ModelVisual3D? selectedFaceVisual;
    private int? selectedFaceIndex;
    private Point lastMousePosition;
    private bool isOrbiting;
    private bool isPanning;
    private bool mouseMovedDuringDrag;
    private string? loadedFilePath;

    public MainWindow()
    {
        InitializeComponent();
        cameraController = new OrbitCameraController(SceneCamera);
        RegisterAxisRows();
        LoadInitialMesh();
    }

    /// <summary>
    /// Loads a sample mesh on startup so the user immediately sees something in the
    /// viewport (no need to open the file dialog). If <c>SampleFiles/cube.obj</c> is
    /// not available next to the executable, falls back to the in-memory preview.
    /// </summary>
    private void LoadInitialMesh()
    {
        var samplePath = ResolveSamplePath("cube.obj");
        if (samplePath is not null)
        {
            try
            {
                var mesh = new ObjParser().Parse(File.ReadAllText(samplePath));
                RenderMesh(mesh, isPreview: false, filePath: samplePath);
                System.Diagnostics.Debug.WriteLine($"[MeshViewer] Auto-loaded sample: {samplePath}");
                return;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MeshViewer] Failed to auto-load {samplePath}: {ex.Message}");
            }
        }

        RenderMesh(CreatePreviewMesh(), isPreview: true);
    }

    private static string? ResolveSamplePath(string fileName)
    {
        // 1) Same folder as the executable (bin/Debug/.../SampleFiles/cube.obj)
        var exeDir = AppContext.BaseDirectory;
        var direct = Path.Combine(exeDir, "SampleFiles", fileName);
        if (File.Exists(direct)) return direct;

        // 2) Project-relative (../../../../SampleFiles/...)
        var relative = Path.Combine(exeDir, "..", "..", "..", "..", "SampleFiles", fileName);
        if (File.Exists(relative)) return Path.GetFullPath(relative);

        // 3) Walk up from cwd looking for a SampleFiles folder
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        for (var i = 0; i < 8 && dir is not null; i++, dir = dir.Parent)
        {
            var candidate = Path.Combine(dir.FullName, "SampleFiles", fileName);
            if (File.Exists(candidate)) return candidate;
        }

        return null;
    }

    private void RegisterAxisRows()
    {
        // Walk the visual tree once after InitializeComponent to map slider-name -> control
        var count = 0;
        foreach (var row in EnumerateVisualChildren<AxisRowControl>(this))
        {
            if (string.IsNullOrEmpty(row.SliderName) || axisRows.ContainsKey(row.SliderName))
            {
                continue;
            }
            axisRows[row.SliderName] = row;
            row.ValueChanged += (_, _) => ApplyMeshTransform();
            row.ValueCommitted += (_, _) => UpdateCameraStatus();
            count++;
        }
        System.Diagnostics.Debug.WriteLine($"[MeshViewer] RegisterAxisRows: found {count} controls, axisRows.Count = {axisRows.Count}");
    }

    private static IEnumerable<T> EnumerateVisualChildren<T>(DependencyObject parent) where T : DependencyObject
    {
        var count = VisualTreeHelper.GetChildrenCount(parent);
        for (var i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T match)
            {
                yield return match;
            }
            foreach (var deeper in EnumerateVisualChildren<T>(child))
            {
                yield return deeper;
            }
        }
    }

    private void RenderMesh(Mesh mesh, bool isPreview = false, string? filePath = null)
    {
        currentMesh = mesh;
        selectedFaceIndex = null;
        loadedFilePath = filePath;

        sceneLights = new Model3DGroup();
        sceneLights.Children.Add(new AmbientLight(Color.FromRgb(180, 180, 180)));
        sceneLights.Children.Add(new DirectionalLight(Colors.White, new Vector3D(-1, -2, -3)));
        sceneLights.Children.Add(new DirectionalLight(Color.FromRgb(220, 220, 240), new Vector3D(2, 1, 2)));

        RenderMeshVisual(null);

        MainViewport.Children.Clear();
        MainViewport.Children.Add(new ModelVisual3D { Content = sceneLights });
        AddMeshVisualsToViewport();
        UpdateMeshStats();
        UpdateSceneCard();
        UpdateFilePath();
        ResetView();
    }

    private void RenderMeshVisual(int? selectedFaceIndex)
    {
        if (currentMesh is null)
        {
            return;
        }

        var isWireframe = WireframeToggle.IsChecked == true;
        var meshMaterial = new DiffuseMaterial(new SolidColorBrush(
            isWireframe ? Color.FromRgb(180, 200, 220) : Color.FromRgb(86, 156, 214)));
        var wireThickness = Math.Max(currentMesh.BoundingBox?.MaxDimension ?? 1, 1) * 0.008;
        var geometry = isWireframe
            ? WireframeGeometryBuilder.Build(currentMesh, wireThickness)
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

        try
        {
                var content = File.ReadAllText(dialog.FileName);
                var mesh = new ObjParser().Parse(content);
                if (mesh.Faces.Count == 0)
                {
                    throw new InvalidOperationException("The OBJ file does not contain any supported renderable faces.");
                }

                System.Diagnostics.Debug.WriteLine(
                    $"[MeshViewer] Loaded '{dialog.FileName}' → {mesh.Vertices.Count} vertices, {mesh.Faces.Count} faces");
                RenderMesh(mesh, isPreview: false, filePath: dialog.FileName);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this,
                $"Failed to load OBJ file:\n\n{ex.Message}",
                "3D Mesh Viewer",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    private void RenderModeToggle_Changed(object sender, RoutedEventArgs e)
    {
        if (currentMesh is null)
        {
            return;
        }

        RebuildScene();
    }

    private double GetAxis(string sliderName) => axisRows.TryGetValue(sliderName, out var row) ? row.Value : DefaultAxisValue(sliderName);

    /// <summary>
    /// Fallback value used when the axis row has not been registered yet (e.g. before
    /// the visual tree has been realized). Scale defaults to 1 (identity), the others
    /// default to 0 so a half-initialized window never collapses the mesh to a point.
    /// </summary>
    private static double DefaultAxisValue(string sliderName) =>
        sliderName.StartsWith("Scale", StringComparison.Ordinal) ? 1.0 : 0.0;

    private void ApplyMeshTransform()
    {
        if (meshVisual is null)
        {
            return;
        }

        var transform = ObjectTransformBuilder.Build(new ObjectTransform(
            GetAxis("RotateXSlider"),
            GetAxis("RotateYSlider"),
            GetAxis("RotateZSlider"),
            GetAxis("TranslateXSlider"),
            GetAxis("TranslateYSlider"),
            GetAxis("TranslateZSlider"),
            GetAxis("ScaleXSlider"),
            GetAxis("ScaleYSlider"),
            GetAxis("ScaleZSlider")));

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
            ? Math.Max(MainViewport.ActualWidth / MainViewport.ActualHeight, 0.2)
            : 1;

        cameraController.FitToView(bounds, aspectRatio);
        UpdateCameraStatus();
    }

    private void ResetViewButton_Click(object sender, RoutedEventArgs e) => ResetView();

    private void Viewport_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        // Re-fit when the viewport gets a real size for the first time (the
        // constructor calls ResetView before layout, so the initial fit uses
        // a placeholder aspect ratio). Also re-fit on every resize.
        if (currentMesh?.BoundingBox is not null && e.NewSize.Width > 0 && e.NewSize.Height > 0)
        {
            ResetView();
        }
    }

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

        UpdateCameraStatus();
    }

    private void MainViewport_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        cameraController.Zoom(e.Delta);
        UpdateCameraStatus();
    }

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
        UpdateSelectionInfo();
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
        if (currentMesh is null)
        {
            MeshStatsText.Text = "No mesh loaded";
            return;
        }

        MeshStatsText.Text = $"{currentMesh.Vertices.Count:N0} vertices · {currentMesh.Faces.Count:N0} faces";
        CameraStatusText.Text = "Camera · ready";
    }

    private void UpdateSceneCard()
    {
        if (currentMesh is null)
        {
            MeshNameText.Text = "No mesh";
            VertexCountText.Text = "—";
            FaceCountText.Text = "—";
            WidthText.Text = "—";
            HeightText.Text = "—";
            return;
        }

        MeshNameText.Text = loadedFilePath is null
            ? "Preview mesh"
            : Path.GetFileName(loadedFilePath);
        VertexCountText.Text = currentMesh.Vertices.Count.ToString("N0");
        FaceCountText.Text = currentMesh.Faces.Count.ToString("N0");

        if (currentMesh.BoundingBox is { } bounds)
        {
            WidthText.Text = $"{bounds.Width:0.###}";
            HeightText.Text = $"{bounds.Height:0.###}";
        }
        else
        {
            WidthText.Text = "—";
            HeightText.Text = "—";
        }
    }

    private void UpdateSelectionInfo()
    {
        SelectionInfoText.Text = selectedFaceIndex is null
            ? "None"
            : $"Face #{selectedFaceIndex.Value:N0}";
        SelectionInfoText.Foreground = selectedFaceIndex is null
            ? (Brush)FindResource("TextMutedBrush")
            : (Brush)FindResource("DangerBrush");
    }

    private void UpdateFilePath()
    {
        FilePathText.Text = loadedFilePath ?? string.Empty;
    }

    private void UpdateCameraStatus()
    {
        if (CameraDistanceText is not null)
            CameraDistanceText.Text = cameraController.Distance.ToString("0.00");
        if (CameraTargetText is not null)
            CameraTargetText.Text = $"{cameraController.Target.X:0.##}, {cameraController.Target.Y:0.##}, {cameraController.Target.Z:0.##}";
        if (CameraStatusText is not null)
            CameraStatusText.Text = $"Camera · d={cameraController.Distance:0.##}";
        UpdateViewportHud();
    }

    private void UpdateViewportHud()
    {
        if (ViewportHudText is null) return;
        var p = SceneCamera.Position;
        var l = SceneCamera.LookDirection;
        ViewportHudText.Text =
            $"pos=({p.X:0.00},{p.Y:0.00},{p.Z:0.00})  d={cameraController.Distance:0.00}  fov={SceneCamera.FieldOfView:0}°\n" +
            $"mesh v={currentMesh?.Vertices.Count ?? 0} f={currentMesh?.Faces.Count ?? 0}";
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
