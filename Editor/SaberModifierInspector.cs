using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SaberFactory.ProjectComponents;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SaberModifierCollection))]
public class SaberModifierInspector : Editor
{
    private static readonly Color SlightGreen = new Color(0.850f, 0.988f, 0.733f);

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var visibilityModList = serializedObject.FindProperty("VisibilityModifiers");
        var componentModList = serializedObject.FindProperty("ComponentModifiers");
        var transformModList = serializedObject.FindProperty("TransformModifiers");

        var compList = serializedObject.FindProperty("Objects");

        var header = EditorGUIUtility.IconContent("d_scenevis_visible_hover@2x");
        header.text = "  Visibility Modifier";

        DrawList(visibilityModList, header, mod =>
        {
            EditorGUILayout.PropertyField(mod.FindPropertyRelative("Name"), EditorStyles.boldFont);
            EditorGUILayout.PropertyField(mod.FindPropertyRelative("DefaultValue"),
                new GUIContent("Visible by default"));
            EditorGUI.indentLevel += 1;
            EditorGUILayout.PropertyField(mod.FindPropertyRelative("Objects"));
            EditorGUI.indentLevel -= 1;
        });

        header = EditorGUIUtility.IconContent("d_MoveTool On@2x");
        header.text = "  Transform Modifier";

        DrawList(transformModList, header, mod =>
        {
            EditorGUILayout.PropertyField(mod.FindPropertyRelative("Name"));
            EditorGUI.indentLevel += 1;
            EditorGUILayout.PropertyField(mod.FindPropertyRelative("Objects"));
            EditorGUI.indentLevel -= 1;
        });

        header = EditorGUIUtility.IconContent("d_FilterByType@2x");
        header.text = "  Component Modifier";

        DrawList(componentModList, header, mod =>
        {
            EditorGUILayout.PropertyField(mod.FindPropertyRelative("Name"));
            EditorGUI.indentLevel += 1;
            var objectProp = mod.FindPropertyRelative("Object");
            EditorGUILayout.PropertyField(objectProp);

            var obj = objectProp.objectReferenceValue as GameObject;
            if (obj != null)
            {
                var typeList = obj.GetComponents<Component>().Select(x => x.GetType().Name).ToList();
                var selected = 0;
                var compTypeProp = mod.FindPropertyRelative("ComponentType");
                var compType = compTypeProp.stringValue;
                if (!string.IsNullOrEmpty(compType))
                {
                    selected = typeList.IndexOf(compType);
                }
                selected = EditorGUILayout.Popup("Type", selected == -1 ? 0 : selected, typeList.ToArray());
                compTypeProp.stringValue = typeList[selected];
            }

            EditorGUI.indentLevel -= 1;
        });

        GUILayout.BeginVertical("Add new", "window", GUILayout.Height(10));
        GUILayout.BeginVertical("box");

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Visibility"))
        {
            visibilityModList.InsertArrayElementAtIndex(0);
        }
        if (GUILayout.Button("Transform"))
        {
            transformModList.InsertArrayElementAtIndex(0);
        }
        if (GUILayout.Button("Component"))
        {
            componentModList.InsertArrayElementAtIndex(0);
        }
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
        GUILayout.EndVertical();

        

        //serializedObject.FindProperty("ObjectJson").stringValue = JsonUtility.ToJson(Target);

        serializedObject.ApplyModifiedProperties();

        GUILayout.Space(10);

        GUI.color = SlightGreen;

        if (GUILayout.Button("Save", GUILayout.Height(25)))
        {
            compList.ClearArray();
            foreach (SerializedProperty prop in visibilityModList)
            {
                prop.FindPropertyRelative("Id").intValue = GUID.Generate().GetHashCode();
                var idxList = prop.FindPropertyRelative("ObjectIndecies");
                idxList.ClearArray();
                foreach (SerializedProperty obj in prop.FindPropertyRelative("Objects"))
                {
                    var idx = compList.arraySize;
                    compList.InsertArrayElementAtIndex(idx);
                    compList.GetArrayElementAtIndex(idx).objectReferenceValue = obj.objectReferenceValue;
                    idxList.InsertArrayElementAtIndex(idxList.arraySize);
                    idxList.GetArrayElementAtIndex(idxList.arraySize - 1).intValue = idx;
                }
            }

            foreach (SerializedProperty prop in componentModList)
            {
                prop.FindPropertyRelative("Id").intValue = GUID.Generate().GetHashCode();
                var idxProp = prop.FindPropertyRelative("ObjectIndex");
                var obj = prop.FindPropertyRelative("Object").objectReferenceValue;
                if (obj == null) continue;
                var idx = compList.arraySize;
                compList.InsertArrayElementAtIndex(idx);
                compList.GetArrayElementAtIndex(idx).objectReferenceValue = obj;
                idxProp.intValue = idx;
            }

            foreach (SerializedProperty prop in transformModList)
            {
                prop.FindPropertyRelative("Id").intValue = GUID.Generate().GetHashCode();
                var idxList = prop.FindPropertyRelative("ObjectIndecies");
                idxList.ClearArray();
                foreach (SerializedProperty obj in prop.FindPropertyRelative("Objects"))
                {
                    var idx = compList.arraySize;
                    compList.InsertArrayElementAtIndex(idx);
                    compList.GetArrayElementAtIndex(idx).objectReferenceValue = obj.objectReferenceValue;
                    idxList.InsertArrayElementAtIndex(idxList.arraySize);
                    idxList.GetArrayElementAtIndex(idxList.arraySize - 1).intValue = idx;
                }
            }

            serializedObject.ApplyModifiedProperties();

            Save();
        }

        GUI.color = Color.white;

        //if (GUILayout.Button("Print Info"))
        //{
        //    Debug.LogWarning($"Instance ID: {Target.GetInstanceID()}");
        //    //Debug.LogWarning($"Instance ID: {}");
        //}
    }

    private void DrawList(SerializedProperty list, GUIContent header, Action<SerializedProperty> modCallback)
    {
        for (int i = 0; i < list.arraySize; i++)
        {
            var mod = list.GetArrayElementAtIndex(i);
            GUILayout.Space(10);

            GUILayout.BeginVertical(header, "window", GUILayout.Height(10));
            GUILayout.BeginVertical("box");

            var clr = Color.cyan;
            clr.a = 0.8f;
            GUI.color = clr;

            GUILayout.Space(5);

            modCallback(mod);

            GUILayout.Space(5);

            clr = Color.red;
            clr.a = 0.8f;
            GUI.color = clr;
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Remove", GUILayout.Width(100)))
            {
                list.DeleteArrayElementAtIndex(i);
            }
            EditorGUILayout.EndHorizontal();


            GUILayout.EndVertical();
            GUILayout.EndVertical();
            GUI.color = Color.white;
            GUILayout.Space(10);
        }
    }

    private void Save()
    {
        Target.ObjectJson = "";
        Target.ObjectJson = JsonUtility.ToJson(Target);
        Debug.Log("Wrote Object JSON");
    }

    private void OnEnable()
    {
        if (target == null)
        {
            return;
        }

        Target = (SaberModifierCollection)target;
    }

    public SaberModifierCollection Target { get; set; }
}