using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

static class SaberProjectSettingsRegister
{
    [SettingsProvider]
    public static SettingsProvider CreateSaberProjectSettingsProvider()
    {
        // First parameter is the path in the Settings window.
        // Second parameter is the scope of this setting: it only appears in the Project Settings window.
        var provider = new SettingsProvider("Project/Saber Exporter", SettingsScope.Project)
        {
            guiHandler = (searchContext) =>
            {
                var settings = SaberProjectSettings.GetSerializedSettings();
                EditorGUIUtility.labelWidth = 200;
                EditorGUILayout.Space(10);
                EditorGUILayout.PropertyField(settings.FindProperty("BeatSaberPath"), new GUIContent("Beat Saber Path"));
                EditorGUILayout.PropertyField(settings.FindProperty("Author"), new GUIContent("Author Name"));

                GUILayout.Space(10);
                GUILayout.Label("Optional", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(settings.FindProperty("CreateRightSaberOnExport"), new GUIContent("Create RightSaber on export"));

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.PropertyField(settings.FindProperty("ExportFilename"), new GUIContent("Export Filename"));
                GUILayout.Label("Available templates: {SaberName}, {SaberAuthor}");
                GUILayout.Label("Examples: \"{SaberAuthor}_{SaberName}.saber\", \"TM_{SaberName}.saber\"");
                EditorGUILayout.EndVertical();

                GUILayout.Space(10);
                EditorGUILayout.PropertyField(settings.FindProperty("ShowOverlay"), new GUIContent("Show Overlay"));

                GUILayout.Space(10);
                EditorGUILayout.BeginVertical("box");
                GUILayout.Label("Needed for launching beat saber within project");
                DrawProp(settings, nameof(SaberProjectSettings.SteamPath), "Steam Path");
                DrawProp(settings, nameof(SaberProjectSettings.AdditionalLaunchArguments), "Additional launch arguments");
                EditorGUILayout.EndVertical();

                settings.ApplyModifiedProperties();
            },

            keywords = new HashSet<string>(new[] { "Beat Saber Path", "Author Name", "Create RightSaber on Export", "Export Filename", "Show Overlay" })
        };

        return provider;
    }

    private static void DrawProp(SerializedObject obj, string propName, string text)
    {
        EditorGUILayout.PropertyField(obj.FindProperty(propName), new GUIContent(text));
    }
}