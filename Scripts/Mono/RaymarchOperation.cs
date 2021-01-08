using UnityEngine;

[ExecuteInEditMode]
public class RaymarchOperation : MonoBehaviour
{
    public enum OpFunction
    {
        None,
        Subtract,
        Intersect,
        Blend
    }

    public OpFunction operation;
    public int childCount;

    public float blendStrength;

    public bool lerpBlend;

    public float minBlend, maxBlend, lerpSpeed;

    void Update()
    {
        if (operation == OpFunction.Blend)
            BlendUpdate();
    }

    void BlendUpdate()
    {
        if (minBlend < 0)
            minBlend = 0;
        if (maxBlend < minBlend)
            maxBlend = minBlend;
        if (lerpBlend)
            blendStrength = Mathf.Lerp(minBlend, maxBlend, Mathf.Abs(Mathf.Sin(Time.time * lerpSpeed)));
    }
}

public struct OperationInfo
{
    public int operation;
    public int childCount;

    public float blendStrength;

    public static int GetSize()
    {
        return sizeof(int) * 2 + sizeof(float) * 1;
    }
}
