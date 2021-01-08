using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]

public class RaymarchControl : SceneViewFilter
{
    Material _material;
    Camera _cam;
    Transform _light;

    List<RaymarchOperation> operations;
    List<RaymarchShape> shapes;

    ComputeBuffer opBuffer;
    ComputeBuffer shapeBuffer;

    int operationCount;
    int children = 0;

    #region Exposed Props
    [Tooltip("Assign the Raymarch shader here")]
    [SerializeField] Shader shader = null;

    [Tooltip("Default pixel return color is set to black when enabled")]
    public bool darkMode = false;
    [Tooltip("Whether or not to calculate the surface normals. Lighting is significantly expensive to calculate.")]
    public bool useLighting = true;
    #endregion

    #region Filter Props
    public Color emissiveColor = Color.white;
    [Tooltip("How many steps each ray has to take before deciding to highlight that particular pixel")]
    public int highlightGradient = 20;
    [Tooltip("How much brighter to highlight the highlighted surface")]
    public float highlightStrength = 3.0f;
    [Tooltip("How much darker to dull unhighlighted surface")]
    public float nonHighlightStrength = 0.5f;

    public enum Filter { None, Highlight }
    public enum HighlightType { ShapeColor, SingleColor }

    [Tooltip("Which filter to apply to the camera")]
    public Filter filter = Filter.None;
    [Tooltip("Should the highlight be one singular color or defined by the shape")]
    public HighlightType highlightType = HighlightType.ShapeColor;
    #endregion

    #region Light Props

    public enum LightMode { Lambertian, CelShaded }

    [Tooltip("The lighting model used with the surface normals to light the shape")]
    public LightMode lightMode = LightMode.Lambertian;

    [Tooltip("How much darker to dull the unlit surface")]
    public float unlitMultiplier = 0.5f;
    [Tooltip("How much brighter to light the lit surface")]
    public float litMultiplier = 1.0f;
    [Tooltip("At what angle between the light direction and surface normals of the shape should the surface be considered unlit")]
    public float flipAngle = 90.0f;

    [Tooltip("Custom Angle is more expensive to render. Default is 90.")]
    public bool customAngle = false;
    #endregion
    public Material Material
    {
        get
        {
            if (!_material && shader)
                _material = new Material(shader);
            return _material;
        }
    }

    public Camera Cam
    {
        get
        {
            if (!_cam)
            {
                _cam = GetComponent<Camera>();
                _cam.depthTextureMode = DepthTextureMode.Depth;
            }
                
            return _cam;
        }
    }

    public Transform Light
    {
        get
        {
            Light l;

            if (!_light)
            {
                l = (Light)FindObjectOfType(typeof(Light));

                if (!l)
                {
                    return _light;
                }

                _light = l.transform;
            }

            return _light;
        }
    }

    static void Blit(RenderTexture source, RenderTexture destination, Material mat, int pass)
    {
        RenderTexture.active = destination;
        mat.SetTexture("_MainTex", source);

        GL.PushMatrix();
        GL.LoadOrtho();
        mat.SetPass(pass);

        GL.Begin(GL.QUADS);

        //Bottom Left
        GL.MultiTexCoord2(0, 0.0f, 0.0f);
        GL.Vertex3(0.0f, 0.0f, 3.0f);

        //Bottom Right
        GL.MultiTexCoord2(0, 1.0f, 0.0f);
        GL.Vertex3(1.0f, 0.0f, 2.0f);

        //Top Right
        GL.MultiTexCoord2(0, 1.0f, 1.0f);
        GL.Vertex3(1.0f, 1.0f, 1.0f);

        //Top Left
        GL.MultiTexCoord2(0, 0.0f, 1.0f);
        GL.Vertex3(0.0f, 1.0f, 0.0f);

        GL.End();
        GL.PopMatrix();
    }


    [ImageEffectOpaque]
    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!Material)
        {
            Graphics.Blit(source, destination);
            return;
        }

        SetMaterialProperties();
        CheckForTransformChange();
        FillBuffer();

        Blit(source, destination, Material, 0);

        opBuffer.Dispose();
        shapeBuffer.Dispose();
    }

    //Returns a matrix containing the corner positions of the camera's view frustum
    Matrix4x4 GetFrustum(Camera cam)
    {
        Matrix4x4 corners = Matrix4x4.identity;

        float camFOV = cam.fieldOfView;
        float camAr = cam.aspect;

        float camRatio = Mathf.Tan(camFOV * .5f * Mathf.Deg2Rad);

        Vector3 right = Vector3.right * camRatio * camAr;
        Vector3 up = Vector3.up * camRatio;

        Vector3 TL = (-Vector3.forward - right + up);
        Vector3 TR = (-Vector3.forward + right + up);
        Vector3 BR = (-Vector3.forward + right - up);
        Vector3 BL = (-Vector3.forward - right - up);

        corners.SetRow(0, TL);
        corners.SetRow(1, TR);
        corners.SetRow(2, BR);
        corners.SetRow(3, BL);

        return corners;
    }

    void FillBuffer()
    {
        OperationInfo[] opInfo = new OperationInfo[operations.Count];

        for (int i = 0; i < operations.Count; i++)
        {
            RaymarchOperation o = operations[i];

            if (o == null)
            {
                UpdateComponentLists();
                o = operations[i];
            }

            opInfo[i] = new OperationInfo()
            {
                operation = (int)o.operation,
                childCount = o.childCount,
                blendStrength = o.blendStrength
            };
        }

        ShapeInfo[] shapeInfo = new ShapeInfo[shapes.Count];

        for (int i = 0; i < shapes.Count; i++)
        {
            RaymarchShape s = shapes[i];

            if (s == null)
            {
                UpdateComponentLists();
                s = shapes[i];
            }

            shapeInfo[i] = new ShapeInfo()
            {
                position = s.transform.position,
                shape = (int)s.shape,
                color = new Vector3(s.color.r, s.color.g, s.color.b),

                sphereRadius = s.sphereRadius,

                boxDimensions = s.boxDimensions,

                roundBoxDimensions = s.roundBoxDimensions,
                roundBoxFactor = s.roundBoxFactor,

                torusInnerRadius = s.torusInnerRadius,
                torusOuterRadius = s.torusOuterRadius,

                coneHeight = s.coneHeight,
                coneRatio = s.coneRatio,
            };
        }

        opBuffer = new ComputeBuffer(opInfo.Length, OperationInfo.GetSize());
        shapeBuffer = new ComputeBuffer(shapeInfo.Length, ShapeInfo.GetSize());

        opBuffer.SetData(opInfo);
        shapeBuffer.SetData(shapeInfo);

        Material.SetBuffer("operations", opBuffer);
        Material.SetBuffer("shapes", shapeBuffer);
    }

    void SetMaterialProperties()
    {
        #region Scene
        Material.SetMatrix("_Frustum", GetFrustum(Cam));
        Material.SetMatrix("_CamMatrix", Cam.cameraToWorldMatrix);
        Material.SetVector("_Light", Light ? Light.forward : Vector3.down);
        Material.SetInt("_OperationCount", operationCount);
        #endregion

        #region Filter
        Material.SetVector("_EmissiveColor", emissiveColor);
        Material.SetInt("_UseLight", Convert.ToInt32(useLighting));
        Material.SetInt("_DarkMode", Convert.ToInt32(darkMode));
        Material.SetInt("_HighlightGradient", highlightGradient);
        Material.SetInt("_Filter", (int)filter);
        Material.SetInt("_Highlight", (int)highlightType);
        Material.SetFloat("_HighlightStrength", highlightStrength);
        Material.SetFloat("_NonHighlightStrength", nonHighlightStrength);
        #endregion

        #region Lighting
        Material.SetInt("_LightMode", (int)lightMode);
        Material.SetFloat("_FlipAngle", flipAngle);
        Material.SetFloat("_LitMultiplier", litMultiplier);
        Material.SetFloat("_UnlitMultiplier", unlitMultiplier);
        Material.SetInt("_CustomAngle", Convert.ToInt32(customAngle));
        #endregion
    }

    void CheckForTransformChange()
    {
        int childTransformCount = 0;

        for (int i = 0; i < Camera.main.transform.childCount; i++)
        {
            childTransformCount++;

            for (int j = 0; j < Camera.main.transform.GetChild(i).transform.childCount; j++)
            {
                childTransformCount++;
            }
        }

        if (children != childTransformCount)
        {
            children = childTransformCount;
            UpdateComponentLists();
        }
    }

    void UpdateComponentLists()
    {
        operations = new List<RaymarchOperation>();
        shapes = new List<RaymarchShape>();

        for (int i = 0; i < Camera.main.transform.childCount; i++)
        {
            int count = 0;

            if (Camera.main.transform.GetChild(i).GetComponent<RaymarchOperation>())
                operations.Add(Camera.main.transform.GetChild(i).GetComponent<RaymarchOperation>());

            for (int j = 0; j < operations[i].transform.childCount; j++)
            {
                if (operations[i].transform.GetChild(j).GetComponent<RaymarchShape>())
                {
                    shapes.Add(operations[i].transform.GetChild(j).GetComponent<RaymarchShape>());
                    count++;
                }
            }
            operations[i].childCount = count;
        }

        operationCount = operations.Count;
    }
}
