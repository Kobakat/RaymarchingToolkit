using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RaymarchControl))]

public class ControlEditor : Editor
{
    public SerializedProperty
    #region Exposed
        shader_prop,
        darkMode_prop,
        useLighting_prop,
    #endregion

    #region Filter 
        emissiveColor_prop,
        highlightGradient_prop,
        nonHighlightStrength_prop,
        highlightStrength_prop,
        filter_prop,
        highlightType_prop,
    #endregion

    #region Lighting
        lightMode_prop,
        flipAngle_prop,
        litMultiplier_prop,
        unlitMultiplier_prop,
        customAngle_prop;
    #endregion

    void OnEnable()
    {
        #region Exposed
        this.shader_prop = serializedObject.FindProperty("shader");
        this.darkMode_prop = serializedObject.FindProperty("darkMode");
        this.useLighting_prop = serializedObject.FindProperty("useLighting");
        #endregion

        #region Filter
        this.emissiveColor_prop = serializedObject.FindProperty("emissiveColor");
        this.highlightGradient_prop = serializedObject.FindProperty("highlightGradient");
        this.nonHighlightStrength_prop = serializedObject.FindProperty("nonHighlightStrength");
        this.highlightStrength_prop = serializedObject.FindProperty("highlightStrength");
        this.filter_prop = serializedObject.FindProperty("filter");
        this.highlightType_prop = serializedObject.FindProperty("highlightType");
        #endregion

        #region Lighting
        this.lightMode_prop = serializedObject.FindProperty("lightMode");
        this.flipAngle_prop = serializedObject.FindProperty("flipAngle");
        this.litMultiplier_prop = serializedObject.FindProperty("litMultiplier");
        this.unlitMultiplier_prop = serializedObject.FindProperty("unlitMultiplier");
        this.customAngle_prop = serializedObject.FindProperty("customAngle");
        #endregion
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(shader_prop);

        DisplaySceneVariables();
        DisplayFilterVariables();
        DisplayLightVariables();

        serializedObject.ApplyModifiedProperties();
    }

    void DisplaySceneVariables()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Scene options", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
     
        EditorGUILayout.PropertyField(useLighting_prop, new GUIContent("Light shapes"));
        EditorGUILayout.PropertyField(darkMode_prop, new GUIContent("Dark mode"));

        EditorGUI.indentLevel--;
    }

    void DisplayFilterVariables()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Filter options", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        EditorGUILayout.PropertyField(filter_prop);

        RaymarchControl.Filter st = (RaymarchControl.Filter)filter_prop.enumValueIndex;

        switch (st)
        {
            case RaymarchControl.Filter.Highlight:
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(nonHighlightStrength_prop, new GUIContent("Base Strength"));
                EditorGUILayout.PropertyField(highlightStrength_prop, new GUIContent("Highlight Strength"));
                EditorGUILayout.PropertyField(highlightGradient_prop, new GUIContent("Gradient Strength"));

                EditorGUILayout.PropertyField(highlightType_prop);
                RaymarchControl.HighlightType ht = (RaymarchControl.HighlightType)highlightType_prop.enumValueIndex;

                

                switch (ht)
                {
                    case RaymarchControl.HighlightType.ShapeColor:
                        break;
                    case RaymarchControl.HighlightType.SingleColor:
                        EditorGUILayout.PropertyField(emissiveColor_prop);
                        break;
                }
                EditorGUI.indentLevel--;
                break;

            default:
                break;
        }

        EditorGUI.indentLevel--;
    }

    void DisplayLightVariables()
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Light options", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        EditorGUILayout.PropertyField(lightMode_prop);

        RaymarchControl.LightMode lt = (RaymarchControl.LightMode)lightMode_prop.enumValueIndex;
        
        switch (lt)
        {
            case RaymarchControl.LightMode.Lambertian:
                break;
            case RaymarchControl.LightMode.CelShaded:
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(litMultiplier_prop, new GUIContent("Lit Multiplier"));
                EditorGUILayout.PropertyField(unlitMultiplier_prop, new GUIContent("Unlit multiplier"));
                EditorGUILayout.PropertyField(customAngle_prop, new GUIContent("Use Custom Angle"));

                if (customAngle_prop.boolValue)
                    EditorGUILayout.PropertyField(flipAngle_prop, new GUIContent("Custom angle"));

                EditorGUI.indentLevel--;
                break;
        }

        EditorGUI.indentLevel--;
    }
}
