using System.IO;
using System.Linq;
using System.Threading;
using CustomSaber;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal.VersionControl;
using UnityEngine;

public class SaberExporterEditor : EditorWindow
{
    private SaberInfo[] _sabers;
    private SaberProjectSettings _saberProjectSettings;

    private BaseTheme _theme;

    [MenuItem("Window/Saber Project/Saber Exporter")]
    public static void ShowWindow()
    {
        GetWindow(typeof(SaberExporterEditor), false, "Saber Exporter");
    }

    private void OnGUI()
    {
        GUILayout.Space(5);

        if (UITools.Button("Settings", 25, Color.HSVToRGB(0.1f, 0.15f, 1)))
        {
            SettingsService.OpenProjectSettings("Project/Saber Exporter");
        }

        var isPathValid = _saberProjectSettings.IsPathValid();

        if (!isPathValid)
        {
            EditorGUILayout.HelpBox("Beat Saber Path is invalid\nPlease adjust the settings", MessageType.Warning);
            GUILayout.Space(10);
        }

        EditorGUI.BeginDisabledGroup(!isPathValid);

        GUILayout.Space(10);


        if (_sabers == null || _sabers.Length < 1)
        {
            UITools.CenterHeader("There aren't any sabers in the scene", _theme.WarningColor);
            if (GUILayout.Button("Create saber using SaberTools", GUILayout.Height(25)))
            {
                SaberTools.OpenSaberTools();
            }
            return;
        }

        foreach (var saber in _sabers)
        {
            if (!saber.IsValid)
            {
                continue;
            }

            GUILayout.BeginVertical(saber.GameObject.name, "window");
            GUI.color = _theme.BackgroundColor;
            GUILayout.BeginVertical("box");
            GUI.color = Color.white;

            var cantExport = false;
            var isWarning = false;

            if (!saber.LeftSaber)
            {
                cantExport = true;
                GUI.color = _theme.ErrorColor;
                GUILayout.Label(" - LeftSaber gameObject is missing", EditorStyles.boldLabel);
                GUI.color = Color.white;
            }

            var saberBounds = saber.Size;

            if (saberBounds.z > SaberTools.SaberLength + 0.1f)
            {
                isWarning = true;
                GUI.color = _theme.WarningColor;
                GUILayout.Label(" - The saber might be too long", EditorStyles.boldLabel);
                GUI.color = Color.white;
            }

            if (saberBounds.z < SaberTools.SaberLength - 0.1f)
            {
                isWarning = true;
                GUI.color = _theme.WarningColor;
                GUILayout.Label(" - The saber might be too short", EditorStyles.boldLabel);
                GUI.color = Color.white;
            }

            if (saberBounds.x > 1.0)
            {
                isWarning = true;
                GUI.color = _theme.WarningColor;
                GUILayout.Label(" - The saber might be too large", EditorStyles.boldLabel);
                GUI.color = Color.white;
            }

            if (saberBounds.x > saberBounds.z || saberBounds.y > saberBounds.z)
            {
                isWarning = true;
                GUI.color = _theme.WarningColor;
                GUILayout.Label(" - Your saber might be rotated incorrectly", EditorStyles.boldLabel);
                GUI.color = Color.white;
            }

            if (!saber.HasTrail)
            {
                isWarning = true;
                GUI.color = _theme.WarningColor;
                GUILayout.Label(" - Your saber doesn't have any trails", EditorStyles.boldLabel);
                GUI.color = Color.white;
            }

            if (saber.HasMultipleTrails)
            {
                isWarning = true;
                GUI.color = _theme.WarningColor;
                GUILayout.Label(" - Try to avoid multiple trails on one saber", EditorStyles.boldLabel);
                GUI.color = Color.white;
            }

            if (saber.HasSaberTransforms)
            {
                isWarning = true;
                GUI.color = _theme.WarningColor;
                GUILayout.Label(" - Your Left/Right-Saber gameobject has transforms applied", EditorStyles.boldLabel);
                GUI.color = Color.white;
            }

            GUILayout.Space(5);

            saber.SaberDescriptor.AuthorName = EditorGUILayout.TextField("Author name", saber.SaberDescriptor.AuthorName);
            saber.SaberDescriptor.SaberName = EditorGUILayout.TextField("Saber name", saber.SaberDescriptor.SaberName);

            EditorGUI.BeginDisabledGroup(!saber.LeftSaber);

            GUILayout.Space(5);
            if (GUILayout.Button("Select"))
            {
                Selection.activeGameObject = saber.GameObject;
            }
            GUI.color = cantExport ? _theme.ErrorColor : (isWarning ? _theme.WarningColor : _theme.SuccessColor);
            if (GUILayout.Button("Export", GUILayout.Height(25)))
            {
                GUI.color = Color.white;
                ExportSaber(saber, false, _saberProjectSettings);
            }

            EditorGUI.EndDisabledGroup();

            GUILayout.EndVertical();
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();

            GUILayout.Space(20);

        }

        EditorGUI.EndDisabledGroup();

    }

    public static void ExportSaber(SaberInfo saber, bool silent = false, SaberProjectSettings saberProjectSettings = null)
    {
        var saberObject = saber.GameObject;

        if (!saberProjectSettings)
        {
            saberProjectSettings = SaberProjectSettings.GetOrCreateSettings();
        }

        if (saberObject)
        {
            var exportName = saberProjectSettings.ExportFilename;
            exportName = exportName.Replace("{SaberName}", saber.SaberDescriptor.SaberName);
            exportName = exportName.Replace("{SaberAuthor}", saber.SaberDescriptor.AuthorName);

            //string path = EditorUtility.SaveFilePanel("Save saber file", "", saber.SaberName + ".saber", "saber");
            var path = Path.Combine(saberProjectSettings.BeatSaberPath, "CustomSabers",
                exportName);

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            if (Directory.Exists(Application.temporaryCachePath))
            {
                Directory.Delete(Application.temporaryCachePath, true);
            }

            if (!string.IsNullOrWhiteSpace(path))
            {
                var fileName = Path.GetFileName(path);
                var folderPath = Path.GetDirectoryName(path);

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                GameObject rightSaber = null;

                // create right saber if it doesn't exist
                if (saberProjectSettings.CreateRightSaberOnExport)
                {
                    saber.CreateRightSaber(out rightSaber);
                }

                Selection.activeObject = saberObject;
                EditorUtility.SetDirty(saber.SaberDescriptor);
                EditorSceneManager.MarkSceneDirty(saberObject.scene);
                EditorSceneManager.SaveScene(saberObject.scene);
                PrefabUtility.SaveAsPrefabAsset(Selection.activeGameObject, "Assets/_CustomSaber.prefab");

                var assetBundleBuild = default(AssetBundleBuild);
                assetBundleBuild.assetNames = new[]
                {
                    "Assets/_CustomSaber.prefab"
                };

                assetBundleBuild.assetBundleName = fileName;

                var selectedBuildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
                var activeBuildTarget = EditorUserBuildSettings.activeBuildTarget;
                BuildPipeline.BuildAssetBundles(Application.temporaryCachePath, new[] { assetBundleBuild }, 0,
                    activeBuildTarget);
                EditorPrefs.SetString("currentBuildingAssetBundlePath", folderPath);
                EditorUserBuildSettings.SwitchActiveBuildTarget(selectedBuildTargetGroup, activeBuildTarget);
                AssetDatabase.DeleteAsset("Assets/_CustomSaber.prefab");
                File.Move(Application.temporaryCachePath + "/" + fileName, path);
                AssetDatabase.Refresh();

                if (!silent)
                {
                    EditorUtility.DisplayDialog("Exportation Successful!", "Exportation Successful!", "OK");
                }

                if (saberProjectSettings.CreateRightSaberOnExport && rightSaber)
                {
                    DestroyImmediate(rightSaber);
                }
            }
            else
            {
                if (!silent)
                {
                    EditorUtility.DisplayDialog("Exportation Failed!", "Path is invalid.", "OK");
                }
            }
        }
        else
        {
            if (!silent)
            {
                EditorUtility.DisplayDialog("Exportation Failed!", "Saber GameObject is missing.", "OK");
            }
        }
    }

    private void OnFocus()
    {
        _sabers = FindObjectsOfType<SaberDescriptor>().Select(x=>new SaberInfo(x)).ToArray();
        _saberProjectSettings = SaberProjectSettings.GetOrCreateSettings();
        _theme = BaseTheme.GetTheme();
    }

    public class SaberInfo
    {
        public SaberDescriptor SaberDescriptor;
        public CustomTrail[] CustomTrails;
        public Transform LeftSaber;
        public Transform RightSaber;

        private Vector3? _size;
        private Transform _transform;

        public GameObject GameObject => SaberDescriptor.gameObject;

        public bool HasTrail => CustomTrails != null && CustomTrails.Length > 0;

        public bool HasMultipleTrails => CustomTrails != null && CustomTrails.Length > 2;

        public bool IsValid => SaberDescriptor;

        public bool HasSaberTransforms;

        public Vector3 Size
        {
            get
            {
                if (!_size.HasValue)
                {
                    _size = SaberTools.GetObjectBounds(GameObject).extents * 2;
                }

                return _size.Value;
            }
        }

        public Transform Transform
        {
            get
            {
                if (!_transform)
                {
                    _transform = SaberDescriptor.transform;
                }

                return _transform;
            }
        }

        public SaberInfo(SaberDescriptor descriptor)
        {
            SaberDescriptor = descriptor;
            CustomTrails = descriptor.gameObject.GetComponentsInChildren<CustomTrail>();
            LeftSaber = Transform.Find("LeftSaber");
            RightSaber = Transform.Find("RightSaber");

            HasSaberTransforms = LeftSaber && !IsTransformClear(LeftSaber) || RightSaber && !IsTransformClear(RightSaber);
        }

        public bool CreateRightSaber(out GameObject createdSaber)
        {
            if (RightSaber)
            {
                createdSaber = null;
                return false;
            }

            var rightSaber = Instantiate(LeftSaber, LeftSaber.parent, false);
            rightSaber.name = "RightSaber";

            // mirror the saber
            var scale = rightSaber.localScale;
            scale.x *= -1;
            rightSaber.localScale = scale;

            foreach (var trail in rightSaber.GetComponentsInChildren<CustomTrail>())
            {
                trail.colorType = ColorType.RightSaber;
            }

            createdSaber = rightSaber.gameObject;
            RightSaber = rightSaber;
            return true;
        }

        private bool IsTransformClear(Transform t)
        {
            return t.localScale == Vector3.one && t.eulerAngles == Vector3.zero;
        }
    }
}