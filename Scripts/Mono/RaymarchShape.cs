using UnityEngine;

public class RaymarchShape : MonoBehaviour
{
    public ShapeInfo shapeInfo;
    public enum Shape
    {
        Sphere,
        Box,
        Torus,
        Cone,
        RoundedBox
    }


    /// <summary>
    /// There properties are used for the inspector
    /// </summary>
    public Shape shape;
    public Color color;

    public float sphereRadius;

    public Vector3 boxDimensions;

    public Vector3 roundBoxDimensions;
    public float roundBoxFactor;

    public float torusOuterRadius;
    public float torusInnerRadius;

    public float coneHeight;
    public Vector2 coneRatio;
}

/// IMPORTANT!
/// GetSize function must be modified when adding/removing
/// Properties to a shape
/// IMPORTANT!
public struct ShapeInfo
{
    public Vector3 position;
    public int shape;
    public Vector3 color;

    public float sphereRadius;

    public Vector3 boxDimensions;

    public Vector3 roundBoxDimensions;
    public float roundBoxFactor;

    public float torusOuterRadius;
    public float torusInnerRadius;

    public float coneHeight;
    public Vector2 coneRatio;

    public static int GetSize()
    {
        return sizeof(float) * 19 + sizeof(int);
    }
}
