using System;
using UnityEditor;
using UnityEngine;

public static class UITools
{
    public static GUIStyle SimplePadding;
    public static GUIStyle BoxStyle;

    static UITools()
    {
        BoxStyle = "box";
        BoxStyle.padding = new RectOffset(10, 10, 10, 10);

        SimplePadding = new GUIStyle();
        SimplePadding.padding = new RectOffset(10, 10, 10, 10);
    }

    public static void BeginSection(Color color)
    {
        GUIStyle box = "box";
        box.padding = new RectOffset(10, 10, 10, 10);

        GUI.color = color;
        GUILayout.BeginVertical(box);
        GUI.color = Color.white;
        GUILayout.Space(5);
    }

    public static void EndSection()
    {
        GUILayout.Space(5);
        GUILayout.EndVertical();
    }

    public static void Header(string text, Color clr, float space = 2)
    {
        GUI.color = clr;
        GUILayout.Label(text, EditorStyles.boldLabel);
        GUI.color = Color.white;
        GUILayout.Space(space);
    }

    public static void CenterHeader(string text, Color clr, float space = 2)
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUI.color = clr;
        GUILayout.Label(text, EditorStyles.boldLabel);
        GUI.color = Color.white;
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(space);
    }

    public static bool FoldoutHeader(bool isOpen, string text, Color clr, float space = 2)
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUI.color = clr;
        isOpen = EditorGUILayout.Foldout(isOpen, text);
        GUI.color = Color.white;
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(space);
        return isOpen;
    }

    public static void Foldout(ref bool isOpen)
    {
        isOpen = EditorGUILayout.Foldout(isOpen, isOpen ? "Close" : "Open");
    }

    public static void ChangedToggle(ref bool isActive, string text, Action<bool> changedAction)
    {
        var newIsActive = EditorGUILayout.Toggle(text, isActive);
        if (newIsActive != isActive)
        {
            changedAction.Invoke(newIsActive);
        }

        isActive = newIsActive;
    }

    public static bool Button(string text, float height = 20, Color? color = null)
    {
        if (color.HasValue)
        {
            GUI.color = color.Value;
        }

        var pressed = GUILayout.Button(text, GUILayout.Height(height));

        GUI.color = Color.white;

        return pressed;
    }
}