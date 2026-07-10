# 3D Mesh Viewer

3D Mesh Viewer is a Windows desktop application for loading, inspecting, transforming, and interacting with Wavefront `.obj` 3D meshes. It is built with WPF and uses a custom geometry pipeline for OBJ parsing, mesh rendering, orbit-camera navigation, object transforms, wireframe display, and triangle selection through ray picking.

## Demo Video

> Add the demo video or GIF here.
>
> Example options:
>
> - YouTube/Vimeo link: `https://...`
> - GitHub uploaded video: drag and drop the video into this section when editing the README on GitHub.
> - Local asset reference: `docs/demo.mp4` or `assets/demo.gif`

<!-- Demo placeholder:

![3D Mesh Viewer demo](assets/demo.gif)

or

https://github.com/user-attachments/assets/your-demo-video-id

-->

## Features

- Load Wavefront `.obj` files from disk.
- Parse vertices, texture coordinates, normals, triangle faces, and quad faces.
- Automatically triangulate quad faces for rendering.
- Render meshes in a WPF `Viewport3D` with `MeshGeometry3D`.
- Auto-load a sample cube on startup when available.
- Fall back to a simple in-memory preview mesh if sample files are unavailable.
- Orbit, pan, zoom, and reset the 3D camera.
- Fit the camera to the loaded mesh bounding box.
- Transform the object in real time with rotation, translation, and scale controls.
- Toggle between solid and wireframe display modes.
- Click a mesh face to select and highlight it using custom ray picking.
- Show mesh statistics including vertex count, face count, width, and height.
- Display current camera distance, camera target, selected face, and loaded file path.
- Include sample OBJ files for quick testing.
- Include xUnit tests for parsing, sample files, and UI helper behavior.

## Screenshots

> Add screenshots here if needed.

```text
assets/screenshot-main.png
assets/screenshot-wireframe.png
assets/screenshot-selection.png
```

## Tech Stack

- C#
- .NET 10
- WPF
- `Viewport3D`
- `PerspectiveCamera`
- `ModelVisual3D`
- `GeometryModel3D`
- `MeshGeometry3D`
- `Transform3DGroup`
- xUnit

## Requirements

- Windows
- .NET 10 SDK
- A desktop environment capable of running WPF applications

## Getting Started

Clone the repository:

```powershell
git clone https://github.com/tuanwannafly/3D-Mesh-Viewer.git
cd 3D-Mesh-Viewer
```

Restore and build the solution:

```powershell
dotnet restore MeshViewer.slnx
dotnet build MeshViewer.slnx
```

Run the application:

```powershell
dotnet run --project MeshViewer/MeshViewer.csproj
```

## Running Tests

Run all tests with:

```powershell
dotnet test MeshViewer.slnx
```

The test suite currently covers:

- OBJ parsing behavior.
- Quad triangulation.
- Bounding-box calculation.
- Sample OBJ file loading.
- Axis control value clamping.

## How To Use

1. Start the application with `dotnet run --project MeshViewer/MeshViewer.csproj`.
2. Use `Load OBJ` to open a `.obj` mesh from disk.
3. Drag with the left mouse button to orbit around the mesh.
4. Hold `Shift` and drag with the left mouse button to pan.
5. Drag with the middle mouse button to pan as an alternative.
6. Scroll the mouse wheel to zoom in and out.
7. Use `Reset View` to fit the camera back to the mesh.
8. Use the Rotate, Translate, and Scale controls to transform the object.
9. Toggle `Wireframe` to switch between solid and wireframe rendering.
10. Click a face in the viewport to select and highlight it.

## Supported OBJ Data

The parser supports the core OBJ data needed by this viewer:

- Vertex positions with `v` records.
- Texture coordinates with `vt` records.
- Vertex normals with `vn` records.
- Triangle faces.
- Quad faces, which are triangulated during parsing.
- Face vertex formats such as `v`, `v/vt`, `v//vn`, and `v/vt/vn`.

The viewer focuses on mesh geometry. Material libraries, textures, smoothing groups, and multi-object scene management are not currently implemented.

## Project Structure

```text
3D-Mesh-Viewer/
|-- MeshViewer.slnx
|-- README.md
|-- SampleFiles/
|   |-- cube.obj
|   |-- pyramid.obj
|   `-- tetrahedron-public-domain.obj
`-- MeshViewer/
    |-- App.xaml
    |-- MeshViewer.csproj
    |-- AssemblyInfo.cs
    |-- Camera/
    |   `-- OrbitCameraController.cs
    |-- Geometry3D/
    |   |-- AABBUtils.cs
    |   |-- MeshGeometryBuilder.cs
    |   |-- MeshRayPicker.cs
    |   |-- ObjectTransform.cs
    |   |-- ObjectTransformBuilder.cs
    |   |-- Ray3D.cs
    |   |-- RayTriangleIntersection.cs
    |   |-- ScreenRayCaster.cs
    |   `-- WireframeGeometryBuilder.cs
    |-- Models/
    |   |-- BoundingBox.cs
    |   |-- Face.cs
    |   |-- FaceVertex.cs
    |   |-- Mesh.cs
    |   |-- Normal.cs
    |   |-- TextureCoordinate.cs
    |   `-- Vertex.cs
    |-- Parsing/
    |   `-- ObjParser.cs
    |-- Themes/
    |   |-- Controls.xaml
    |   `-- Tokens.xaml
    |-- Views/
    |   |-- AxisRowControl.xaml
    |   |-- AxisRowControl.xaml.cs
    |   |-- MainWindow.xaml
    |   `-- MainWindow.xaml.cs
    `-- Tests/
        |-- MeshViewer.Tests.csproj
        |-- Parsing/
        |   |-- ObjParserTests.cs
        |   `-- SampleFilesTests.cs
        `-- Views/
            `-- AxisRowControlTests.cs
```

## Architecture

```text
OBJ file
   |
   v
ObjParser
   |
   v
Mesh model
   |-- Vertices
   |-- Texture coordinates
   |-- Normals
   |-- Faces
   `-- Axis-aligned bounding box
   |
   v
MeshGeometryBuilder / WireframeGeometryBuilder
   |
   v
WPF Viewport3D + PerspectiveCamera
   |
   v
OrbitCameraController
   |
   v
ObjectTransformBuilder
   |
   v
Rendered and transformed mesh
```

Face picking uses a separate custom pipeline:

```text
Mouse click
   |
   v
ScreenRayCaster
   |
   v
World-space ray
   |
   v
AABB broad-phase test
   |
   v
Moller-Trumbore triangle intersection
   |
   v
Closest hit face
   |
   v
Selected face highlight
```

## Key Components

### OBJ Parser

`ObjParser` reads supported Wavefront OBJ records and converts them into the internal `Mesh` model. It stores vertex positions, texture coordinates, normals, and faces. Quad faces are split into triangles so they can be rendered through WPF mesh geometry.

### Mesh Model

The mesh model contains the parsed geometry data and maintains an axis-aligned bounding box. The bounding box is used for camera fitting, viewport setup, and ray-picking optimization.

### Rendering

The application renders the mesh through WPF `Viewport3D`. Solid rendering uses triangle geometry built from the parsed mesh. Wireframe rendering builds thin geometry along mesh edges so the object can be inspected in line form.

### Camera Controls

`OrbitCameraController` manages the `PerspectiveCamera`. It supports orbiting around a target point, panning, zooming, and fitting the camera to the loaded mesh bounds.

### Object Transforms

The inspector panel exposes rotation, translation, and scale controls for the X, Y, and Z axes. These values are converted into a WPF transform group and applied to both the mesh and selected-face highlight.

### Ray Picking

The application does not rely on WPF built-in hit testing for face selection. Instead, it creates a ray from the clicked screen position and tests it against the mesh manually.

The picking flow uses two stages:

- AABB broad-phase: quickly rejects clicks that do not intersect the mesh bounding box.
- Triangle narrow-phase: tests individual triangles using the Moller-Trumbore ray-triangle intersection algorithm.

When multiple triangles are hit, the closest hit is selected and highlighted.

## Sample Files

The `SampleFiles/` directory includes small OBJ files that are useful for testing the viewer quickly:

- `cube.obj`
- `pyramid.obj`
- `tetrahedron-public-domain.obj`

The application attempts to load `cube.obj` automatically on startup. If the sample file cannot be found, it creates a simple preview triangle in memory.

## Demo Checklist

Use this checklist when recording the demo video:

1. Start the application.
2. Show the sample cube loaded on startup.
3. Click `Load OBJ` and open a file from `SampleFiles/`.
4. Orbit the camera with left mouse drag.
5. Pan with `Shift + left drag` or middle mouse drag.
6. Zoom with the mouse wheel.
7. Use `Reset View`.
8. Rotate the object on X, Y, and Z axes.
9. Translate the object on X, Y, and Z axes.
10. Scale the object on X, Y, and Z axes.
11. Toggle wireframe mode.
12. Click a face and show the red selected-face highlight.
13. Show the scene statistics and selected face information.

## Current Limitations

- Only `.obj` mesh geometry is supported.
- `.mtl` material files are not loaded.
- Texture rendering is not implemented.
- The scene currently focuses on one loaded mesh at a time.
- Very large meshes may benefit from spatial acceleration structures in future versions.

## Future Improvements

- Add `.mtl` material support.
- Add texture loading and rendering.
- Add support for multiple objects in one scene.
- Add asynchronous loading with progress feedback for large files.
- Add octree or BVH acceleration for faster picking on large meshes.
- Improve wireframe rendering with a shader-based or dedicated line-rendering approach.
- Add export options for transformed meshes.

## License

No license file is currently included in this repository. Add a license before distributing or publishing the project for external use.
