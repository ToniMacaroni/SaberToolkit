using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SaberDescriptor))]
[CanEditMultipleObjects]
public class SaberDescriptorEditor : Editor
{
    void OnEnable()
    {
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawPropertiesExcluding(serializedObject, "m_Script");
        GUILayout.Space(5);
        if (UITools.Button("Quick Export", 23))
        {
            SaberExporterEditor.ExportSaber(new SaberExporterEditor.SaberInfo(serializedObject.targetObject as SaberDescriptor));
        }
        serializedObject.ApplyModifiedProperties();
    }
}