using System;
using CustomSaber;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class SaberTools : EditorWindow
{
    public static SaberTools Instance;

    public const float SaberLength = 1.179f;
    private const float SaberOffset = 0.1745f;

    public bool ShowSaberGuides = true;
    public bool ShowTrailGuides = true;
    public bool ShowTrailPreview = false;
    public float TrailPreviewLength = 0.5f;
    public bool PreviewCC = true;
    public bool BeatSaberLookActive;

    public Color CustomColorLeft = Color.red;
    public Color CustomColorRight = Color.cyan;

    private SaberProjectSettings _saberProjectSettings;

    private string _templateText = "NewSaber";
    private GameObject _templatePrefab;
    private bool _createRightSaber;
    private string _spacingBetweenSabers = "0.3";
    private Material _trailMaterial;
    private int _trailLength = 16;
    private float _trailWidth = 0.5f;

    private bool _isCreateSaberOpen;
    private bool _isGuidesOpen;
    private bool _isFixingOpen;
    private bool _isBloomTabOpen;
    private bool _isOtherToolsOpen;

    private Vector2 _scrollPos = Vector2.zero;

    private SaberDescriptor _selectedDescriptor;

    private BaseTheme _theme;

    [MenuItem("Window/Saber Project/Saber Tools")]
    public static void OpenSaberTools()
    {
        Instance = GetWindow<SaberTools>(false, "Saber Tools");
    }

    public void OnGUI()
    {
        GUILayout.Space(10);

        _scrollPos = GUILayout.BeginScrollView(_scrollPos);

        UITools.BeginSection(_theme.BackgroundColor);
        UITools.CenterHeader("Visualizers", _theme.HeaderColor);
        UITools.Foldout(ref _isGuidesOpen);
        if (_isGuidesOpen)
        {
            UITools.ChangedToggle(ref ShowSaberGuides, "Show Guides", val =>
            {
                SceneView.RepaintAll();
            });

            UITools.ChangedToggle(ref ShowTrailGuides, "Show Trail Guides", val =>
            {
                SceneView.RepaintAll();
            });

            UITools.ChangedToggle(ref ShowTrailPreview, "Show Trail Preview", val =>
            {
                SceneView.RepaintAll();
            });

            if (ShowTrailPreview)
            {
                var newTrailPreviewLength = EditorGUILayout.Slider("- Preview length", TrailPreviewLength, 0, 1);
                if (newTrailPreviewLength != TrailPreviewLength)
                {
                    SceneView.RepaintAll();
                }
                TrailPreviewLength = newTrailPreviewLength;

                UITools.ChangedToggle(ref PreviewCC, "- Preview vertex CC", val =>
                {
                    SceneView.RepaintAll();
                });
            }

        }
        UITools.EndSection();

        UITools.BeginSection(_theme.BackgroundColor);
        UITools.CenterHeader("Create Saber", _theme.HeaderColor);
        UITools.Foldout(ref _isCreateSaberOpen);
        if (_isCreateSaberOpen)
        {
            UITools.Header("General", Color.cyan);
            _templateText = EditorGUILayout.TextField("Name", _templateText);
            Space(2);
            _createRightSaber = EditorGUILayout.Toggle("Create RightSaber", _createRightSaber);
            if (_createRightSaber)
            {
                _spacingBetweenSabers = EditorGUILayout.TextField("Spacing between sabers", _spacingBetweenSabers);
            }
            _templatePrefab =
                (GameObject) EditorGUILayout.ObjectField("Template Prefab", _templatePrefab, typeof(GameObject), false);
            Space();
            UITools.Header("Trails", Color.cyan);
            _trailMaterial = (Material) EditorGUILayout.ObjectField("Trail Material", _trailMaterial, typeof(Material), false);
            _trailLength = EditorGUILayout.IntField("Trail Length", _trailLength);
            _trailWidth = EditorGUILayout.Slider("Trail Width", _trailWidth, 0, SaberLength);
            Space(10);
            if (GUILayout.Button("Create Template", GUILayout.Height(20)))
            {
                CreateTemplate();
            }
        }
        UITools.EndSection();

        UITools.BeginSection(_theme.BackgroundColor);
        UITools.CenterHeader("Fixing", _theme.HeaderColor);
        UITools.Foldout(ref _isFixingOpen);
        if (_isFixingOpen)
        {
            if (UITools.Button("Fix Length"))
            {
                FixLength();
            }
        }
        UITools.EndSection();

        UITools.BeginSection(_theme.BackgroundColor);
        UITools.CenterHeader("Beat Saber look", _theme.HeaderColor);
        UITools.Foldout(ref _isBloomTabOpen);
        if (_isBloomTabOpen)
        {
            UITools.ChangedToggle(ref BeatSaberLookActive, "Bloom", newVal =>
            {
                var cam = Camera.main;
                var bloomScript = cam.GetComponent<BloomEffect>();
                if (!bloomScript)
                {
                    bloomScript = cam.gameObject.AddComponent<BloomEffect>();
                }

                bloomScript.enabled = newVal;
            });

            UITools.Header("Custom Colors", Color.cyan);
            EditorGUILayout.BeginHorizontal();
            CustomColorLeft = EditorGUILayout.ColorField("Left", CustomColorLeft);
            Space(20);
            CustomColorRight = EditorGUILayout.ColorField("Right", CustomColorRight);
            EditorGUILayout.EndHorizontal();
        }
        UITools.EndSection();

        UITools.BeginSection(_theme.BackgroundColor);
        UITools.CenterHeader("Other tools", _theme.HeaderColor);
        UITools.Foldout(ref _isOtherToolsOpen);
        if (_isOtherToolsOpen)
        {
            if (UITools.Button("Select all renderers"))
            {
                var go = Selection.activeGameObject;
                if (go)
                {
                    var gos = new List<GameObject>();
                    foreach (var meshRenderer in go.GetComponentsInChildren<MeshRenderer>())
                    {
                        gos.Add(meshRenderer.gameObject);
                    }

                    Selection.objects = gos.ToArray();
                }
            }

            Space();
            GUILayout.Label("Select trail transform");

            GUILayout.BeginHorizontal();
            if (UITools.Button("Bottom"))
            {
                SelectTrailTransform(Selection.activeGameObject, false);
            }

            if (UITools.Button("Top"))
            {
                SelectTrailTransform(Selection.activeGameObject, true);
            }
            GUILayout.EndHorizontal();

            Space(10);

            if (UITools.Button("Create spinning anim", 23))
            {
                if (Selection.activeGameObject)
                {
                    var animCreator = AnimCreatorWindow.Open();
                    animCreator.Setup(new AnimCreatorWindow.InitData
                    {
                        GameObject = Selection.activeGameObject
                    });
                }
                else
                {
                    SaberProjectOverlay.ShowNotification("Select a gameobject first");
                }
            }
        }
        UITools.EndSection();

        GUILayout.EndScrollView();
    }

    public void OnFocus()
    {
        _saberProjectSettings = SaberProjectSettings.GetOrCreateSettings();

        if (Selection.activeGameObject)
        {
            _selectedDescriptor = Selection.activeGameObject.GetComponent<SaberDescriptor>();
        }
        else
        {
            _selectedDescriptor = null;
        }

        _theme = BaseTheme.GetTheme();
    }

    public void FixLength()
    {
        Vector3 Abs(Vector3 vec)
        {
            return new Vector3(Mathf.Abs(vec.x), Mathf.Abs(vec.y), Mathf.Abs(vec.z));
        }

        foreach (var gameObject in Selection.gameObjects)
        {
            var t = gameObject.transform;
            var localToWorld = t.localToWorldMatrix;
            var worldToLocal = t.worldToLocalMatrix;

            var ogScale = Abs(localToWorld.rotation*t.localScale);
            ogScale.z = 1f;
            t.localScale = Abs(worldToLocal.rotation * ogScale);

            var bounds = GetObjectBounds(gameObject).extents * 2;
            var targetZ = SaberLength / bounds.z;
            ogScale.z = targetZ;
            t.localScale = Abs(worldToLocal.rotation * ogScale);
        }
    }

    private void SelectTrailTransform(GameObject root, bool top)
    {
        var trails = new List<GameObject>();

        foreach (var trail in root.GetComponentsInChildren<CustomTrail>())
        {
            var go = top ? trail.PointEnd : trail.PointStart;
            if (go)
            {
                trails.Add(go.gameObject);
            }
        }

        Selection.objects = trails.ToArray();
    }

    public void CreateTemplate()
    {
        var rootGo = new GameObject(_templateText);
        var saberDescriptor = rootGo.AddComponent<SaberDescriptor>();
        saberDescriptor.SaberName = _templateText;

        if (_saberProjectSettings)
        {
            saberDescriptor.AuthorName = _saberProjectSettings.Author;
        }

        var leftSaberGo = new GameObject("LeftSaber");
        leftSaberGo.transform.parent = rootGo.transform;
        CreateTrail(leftSaberGo, ColorType.LeftSaber);
        AddPrefab(leftSaberGo.transform);

        if (_createRightSaber)
        {
            if (!float.TryParse(_spacingBetweenSabers, out var spacingBetweenSabers))
            {
                spacingBetweenSabers = 0.3f;
            }

            leftSaberGo.transform.position = new Vector3(-spacingBetweenSabers, 0, 0);

            var rightSaberGo = new GameObject("RightSaber");
            rightSaberGo.transform.parent = rootGo.transform;
            CreateTrail(rightSaberGo, ColorType.RightSaber);
            rightSaberGo.transform.position = new Vector3(spacingBetweenSabers, 0, 0);

            AddPrefab(rightSaberGo.transform);
        }

        Selection.activeGameObject = rootGo;
    }

    private void AddPrefab(Transform t)
    {
        if (!_templatePrefab)
        {
            return;
        }

        var prefab = Instantiate(_templatePrefab, t, false);
    }

    public void CreateTrail(GameObject saberGo, ColorType colorType)
    {
        var trail = saberGo.AddComponent<CustomTrail>();
        trail.TrailMaterial = _trailMaterial;
        trail.Length = _trailLength;
        trail.colorType = colorType;

        var trailGuides = new GameObject("Trail Guides").transform;
        trailGuides.parent = saberGo.transform;

        var trailGuideTop = new GameObject("Trail Top").transform;
        trailGuideTop.parent = trailGuides;
        trailGuideTop.localPosition = new Vector3(0, 0, SaberLength - SaberOffset);

        var trailGuideBottom = new GameObject("Trail Bottom").transform;
        trailGuideBottom.parent = trailGuides;
        trailGuideBottom.localPosition = new Vector3(0, 0, SaberLength - SaberOffset - _trailWidth);

        trail.PointEnd = trailGuideTop;
        trail.PointStart = trailGuideBottom;
    }

    private void Space(float space = 5)
    {
        GUILayout.Space(space);
    }

    [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
    private static void DrawGizmos(SaberDescriptor descriptor, GizmoType gizmoType)
    {
        if (!Instance || !Instance.ShowSaberGuides)
        {
            return;
        }

        foreach (Transform t in descriptor.transform)
        {
            if (t.name == "LeftSaber")
            {
                DrawSaberGizmo(t, true);
            }
            else if (t.name == "RightSaber")
            {
                DrawSaberGizmo(t, false);
            }
        }
    }

    private static void DrawSaberGizmo(Transform t, bool isLeft)
    {
        Gizmos.color = isLeft ? Color.red : Color.blue;
        Gizmos.DrawWireCube(t.position+new Vector3(0, 0, SaberLength/2-SaberOffset), new Vector3(0.05f, 0.05f, SaberLength));
        Gizmos.color = Color.white;

        var trail = t.GetComponent<CustomTrail>();
        if (trail)
        {
            DrawTrailGizmo(trail, isLeft);
        }
    }

    private static void DrawTrailGizmo(CustomTrail trail, bool isLeft)
    {
        if (!Instance || !Instance.ShowTrailGuides)
        {
            return;
        }

        var trailWidth = trail.PointEnd.position.z - trail.PointStart.position.z;
        var gizmoWidth = 0.3f;

        Gizmos.color = isLeft ? Color.red : Color.blue;
        Gizmos.DrawWireCube(trail.PointStart.position + new Vector3(0.025f+gizmoWidth/2, 0, trailWidth/2), new Vector3(gizmoWidth, 0.05f, trailWidth));
        Gizmos.color = Color.white;
    }

    public static Bounds GetObjectBounds(GameObject g)
    {
        var b = new Bounds(g.transform.position, Vector3.zero);
        foreach (var r in g.GetComponentsInChildren<Renderer>()) b.Encapsulate(r.bounds);
        return b;
    }

    public static void SetGameObjectIcon(GameObject gameObject, GUIContent icon)
    {
        var egu = typeof(EditorGUIUtility);
        var flags = BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic;
        var args = new object[] { gameObject, icon.image };
        var setIcon = egu.GetMethod("SetIconForObject", flags, null, new Type[] { typeof(UnityEngine.Object), typeof(Texture2D) }, null);
        setIcon.Invoke(null, args);
    }
}