using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RaymarchOperation)), CanEditMultipleObjects]
public class OperationEditor : Editor
{
    public SerializedProperty
        operation_Prop,
        blendStrength_Prop,
        lerpBlend_Prop,
        minBlend_Prop,
        maxBlend_Prop,
        lerpSpeed_Prop;



    void OnEnable()
    {
        this.operation_Prop = serializedObject.FindProperty("operation");
        this.blendStrength_Prop = serializedObject.FindProperty("blendStrength");
        this.lerpBlend_Prop = serializedObject.FindProperty("lerpBlend");
        this.minBlend_Prop = serializedObject.FindProperty("minBlend");
        this.maxBlend_Prop = serializedObject.FindProperty("maxBlend");
        this.lerpSpeed_Prop = serializedObject.FindProperty("lerpSpeed");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(operation_Prop);

        RaymarchOperation.OpFunction st = (RaymarchOperation.OpFunction)operation_Prop.enumValueIndex;
        
        EditorGUILayout.Space();
        switch (st)
        {
            case RaymarchOperation.OpFunction.None:
            case RaymarchOperation.OpFunction.Subtract:
            case RaymarchOperation.OpFunction.Intersect:
                break;
            case RaymarchOperation.OpFunction.Blend:
                EditorGUILayout.LabelField("Blend Options", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(blendStrength_Prop, new GUIContent("BlendStrength"));
                EditorGUILayout.PropertyField(lerpBlend_Prop, new GUIContent("Lerp Blend?"));

                if (lerpBlend_Prop.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(minBlend_Prop, new GUIContent("Min Blend Value"));
                    EditorGUILayout.PropertyField(maxBlend_Prop, new GUIContent("Max Blend Value"));
                    EditorGUILayout.PropertyField(lerpSpeed_Prop, new GUIContent("Lerp Speed"));
                }

                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
