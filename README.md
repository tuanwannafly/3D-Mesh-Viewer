# 3D Mesh Viewer

WPF desktop app for loading simple Wavefront `.obj` meshes, viewing them in 3D, transforming the object, and selecting individual triangles with a custom ray-picking pipeline.

## Features

- Load `.obj` files with vertices, texture coordinates, normals, and triangle/quad faces.
- Render meshes in WPF `Viewport3D` using `MeshGeometry3D`.
- Orbit, zoom, pan, and reset the camera around the mesh AABB center.
- Transform the mesh in real time with rotate, translate, and scale sliders.
- Toggle solid/wireframe display mode.
- Click a triangle to highlight it with custom ray-picking, without WPF `HitTest`.
- View mesh stats in the status bar.

## Tech Stack

- .NET 10 WPF
- `Viewport3D`, `ModelVisual3D`, `GeometryModel3D`, `MeshGeometry3D`
- `PerspectiveCamera`, `Point3D`, `Vector3D`, `Matrix3D`, `Transform3DGroup`
- xUnit tests

## Architecture

```text
OBJ File (.obj)
      |
      v
ObjParser (custom)  ->  Mesh { Vertices, Normals, Faces, AABB }
      |                         (AABB is calculated while parsing)
      v
MeshGeometry3D conversion for WPF rendering
      |
      v
Viewport3D + PerspectiveCamera -> OrbitCameraController (rotate/zoom/pan/fit-to-view)
      |
      v
Transform3DGroup (rotate/translate/scale object)
      |
Mouse Click -> Screen-to-World Ray -> AABB broad-phase -> Moller-Trumbore per triangle -> Highlight hit face
```

## Project Structure

```text
MeshViewer/
├─ Models/       Mesh, Vertex, Face, BoundingBox
├─ Parsing/      ObjParser
├─ Geometry3D/   Mesh conversion, transforms, wireframe, ray-picking algorithms
├─ Camera/       OrbitCameraController
├─ Views/        MainWindow.xaml
└─ Tests/        xUnit tests
SampleFiles/     Small OBJ files for parser/rendering checks
```

## Ray Picking

The app does not use WPF built-in hit testing. A mouse click is converted into a world-space ray from the active `PerspectiveCamera` by rebuilding the camera basis from look, up, right vectors and projecting the screen point through the camera field of view.

Before checking triangles, the ray is tested against the mesh AABB using the slab method. This quickly rejects misses by comparing the `t` interval where the ray overlaps each axis-aligned box range.

For triangle hits, the app uses the Moller-Trumbore algorithm. It solves the ray/triangle intersection in barycentric coordinates with cross and dot products. If the determinant is near zero, the ray is parallel to the triangle plane and is rejected. The barycentric values `u` and `v` must remain inside the triangle, and `t` must be non-negative so intersections behind the camera are ignored. When multiple triangles hit, the smallest `t` is selected because it is closest to the camera.

## Running

```powershell
dotnet build MeshViewer.slnx
dotnet run --project MeshViewer/MeshViewer.csproj
```

## Testing

```powershell
dotnet test MeshViewer.slnx
```

## Demo Checklist

A short demo video/GIF should show:

1. Load an OBJ file from `SampleFiles/` with `Load OBJ`.
2. Orbit the camera with left mouse drag.
3. Zoom with mouse wheel and pan with middle mouse drag or `Shift + left drag`.
4. Rotate, translate, and scale the mesh with sliders.
5. Click a triangle and confirm it highlights red.
6. Toggle Wireframe/Solid render mode and use `Reset View`.

## Future Improvements

- Octree acceleration for large meshes, as a 3D analog to QuadTree broad-phase in 2D projects.
- `.mtl` material and texture support.
- Multi-object scene management.
- Better wireframe rendering using shader-based or line-primitive rendering.
- Async loading and progress reporting for large OBJ files.
