namespace MeshViewer.Geometry3D;

public readonly record struct ObjectTransform(
    double RotateX,
    double RotateY,
    double RotateZ,
    double TranslateX,
    double TranslateY,
    double TranslateZ,
    double ScaleX,
    double ScaleY,
    double ScaleZ);
