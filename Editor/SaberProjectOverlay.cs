using System.Diagnostics;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class SaberProjectOverlay
{
    private static bool _isDarkMode;
    private static SaberProjectSettings _settings;

    private static SceneView _currentSceneview;

    static SaberProjectOverlay()
    {
        _isDarkMode = EditorGUIUtility.isProSkin;
        _settings = SaberProjectSettings.GetOrCreateSettings();

        SceneView.duringSceneGui -= DrawGUI;
        SceneView.duringSceneGui += DrawGUI;
    }

    private static void DrawGUI(SceneView sceneView)
    {
        _currentSceneview = sceneView;
        var svEvent = Event.current;

        if (svEvent.type == EventType.Layout)
        {
            if (!_settings.ShowOverlay)
            {
                return;
            }

            var stl = new GUIStyle();
            //stl.normal.background = Texture2D.blackTexture;
            //stl.onNormal.background = Texture2D.blackTexture;
            //stl.padding = new RectOffset(2, 2, 2, 2);

            GUILayout.Window(69420, new Rect(0, 23, sceneView.position.width/2, 40), DrawWindow, "", stl);
        }
    }

    private static void DrawWindow(int id)
    {
        if (_isDarkMode)
        {
            GUI.color = Color.black;
        }
        GUILayout.BeginHorizontal("box");
        GUI.color = Color.white;
        GUILayout.Label("Saber Project v" + SaberToolsUpdater.LocalVersionString);
        GUILayout.Space(5);
        var blk = new Color(1, 1, 1, 0.4f);
        GUI.color = blk;

        if (SaberToolsUpdater.IsUpdateAvailable && !SaberToolsUpdater.IsUpdating)
        {
            if (GUILayout.Button("Update"))
            {
                _ = SaberToolsUpdater.Update();
            }
        }

        if (GUILayout.Button("Settings"))
        {
            SaberProjectSettings.OpenSettingsScreen();
        }

        if (GUILayout.Button("Exporter"))
        {
            SaberExporterEditor.ShowWindow();
        }

        if (GUILayout.Button("Saber Tools"))
        {
            SaberTools.OpenSaberTools();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Start BeatSaber"))
        {

            if (!_settings.IsSteamPathValid())
            {
                _currentSceneview.ShowNotification(new GUIContent("Please set the steam path"), 1f);
            }
            else
            {
                StartBeatSaber();
            }
        }

        GUI.color = Color.white;
        GUILayout.EndHorizontal();
    }

    private static void StartBeatSaber()
    {
        Process.Start(_settings.SteamPath, "-applaunch 620980 " + _settings.AdditionalLaunchArguments);
    }
}