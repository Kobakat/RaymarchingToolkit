using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RaymarchShape)), CanEditMultipleObjects]
public class ShapeEditor : Editor
{

    public SerializedProperty
        shape_Prop,
        color_Prop,
        sphereRadius_Prop,
        boxDimensions_Prop,
        roundBoxDimensions_Prop,
        roundBoxFactor_Prop,
        torusOuterRadius_Prop,
        torusInnerRadius_Prop,
        coneHeight_Prop,
        coneRatio_Prop;


    void OnEnable()
    {
        this.shape_Prop = serializedObject.FindProperty("shape");
        this.color_Prop = serializedObject.FindProperty("color");

        this.sphereRadius_Prop = serializedObject.FindProperty("sphereRadius");

        this.boxDimensions_Prop = serializedObject.FindProperty("boxDimensions");

        this.roundBoxDimensions_Prop = serializedObject.FindProperty("roundBoxDimensions");
        this.roundBoxFactor_Prop = serializedObject.FindProperty("roundBoxFactor");

        this.torusOuterRadius_Prop = serializedObject.FindProperty("torusOuterRadius");
        this.torusInnerRadius_Prop = serializedObject.FindProperty("torusInnerRadius");

        this.coneHeight_Prop = serializedObject.FindProperty("coneHeight");
        this.coneRatio_Prop = serializedObject.FindProperty("coneRatio");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUILayout.PropertyField(color_Prop);
        EditorGUILayout.PropertyField(shape_Prop);
        

        RaymarchShape.Shape st = (RaymarchShape.Shape)shape_Prop.enumValueIndex;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Shape properties", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        switch (st)
        {
            case RaymarchShape.Shape.Sphere:
                EditorGUILayout.PropertyField(sphereRadius_Prop, new GUIContent("Radius"));
                break;

            case RaymarchShape.Shape.Box:
                EditorGUILayout.PropertyField(boxDimensions_Prop, new GUIContent("Dimensions"));
                break;

            case RaymarchShape.Shape.Torus:
                EditorGUILayout.PropertyField(torusInnerRadius_Prop, new GUIContent("Inner Radius"));
                EditorGUILayout.PropertyField(torusOuterRadius_Prop, new GUIContent("Outer Radius"));
                break;

            case RaymarchShape.Shape.Cone:
                EditorGUILayout.PropertyField(coneRatio_Prop, new GUIContent("Ratio"));
                EditorGUILayout.PropertyField(coneHeight_Prop, new GUIContent("Height"));
                break;

            case RaymarchShape.Shape.RoundedBox:
                EditorGUILayout.PropertyField(roundBoxDimensions_Prop, new GUIContent("Dimensions"));
                EditorGUILayout.PropertyField(roundBoxFactor_Prop, new GUIContent("Roundness"));
                break;

        }


        serializedObject.ApplyModifiedProperties();
    }
}