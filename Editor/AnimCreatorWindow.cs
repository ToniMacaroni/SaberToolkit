using System;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.PackageManager.UI;
using UnityEngine;

public class AnimCreatorWindow : EditorWindow
{
    private InitData _currentData;

    private string _folder = "Animations";
    private float _animationSpeed = 1;
    private Vector3 _axis = new Vector3(0, 1, 0);
    private bool _startFromCurrentRotation;
    private float _offset;

    private static readonly Color SectionColor = new Color(0, 0, 0, 0.5f);

    public static AnimCreatorWindow Open()
    {
        var instance = GetWindow<AnimCreatorWindow>(true, "Anim Creator");
        PopupTools.Center(instance, 300, 250);
        return instance;
    }

    public void Setup(InitData initData)
    {
        _currentData = initData;
    }

    private void OnGUI()
    {
        if (!_currentData.GameObject)
        {
            return;
        }

        GUILayout.BeginVertical(UITools.SimplePadding);

        UITools.BeginSection(SectionColor);
        _folder = EditorGUILayout.TextField("Folder", _folder);
        EditorGUILayout.ObjectField("Gameobject", _currentData.GameObject, typeof(GameObject), true);
        UITools.EndSection();
        
        UITools.BeginSection(SectionColor);
        _animationSpeed = EditorGUILayout.FloatField("Speed", _animationSpeed);
        _axis = EditorGUILayout.Vector3Field("Axis", _axis);
        _offset = EditorGUILayout.FloatField("Offset", _offset);
        _offset = Mathf.Max(_offset, 0);
        if (_offset == 0)
        {
            _startFromCurrentRotation = EditorGUILayout.Toggle("Start from current rotation", _startFromCurrentRotation);
        }
        else
        {
            _startFromCurrentRotation = false;
        }
        UITools.EndSection();

        GUILayout.Space(10);
        if (UITools.Button("Create Animation", 23))
        {
            CreateAnim();
        }

        GUILayout.EndVertical();
    }

    private void CreateAnim()
    {
        var folder = Path.Combine(Application.dataPath, _folder);
        Directory.CreateDirectory(folder);

        var filename = IncrementalFilename(folder, _currentData.GameObject.name+".anim");

        var go = _currentData.GameObject.transform;
        if (_offset != 0)
        {
            var newGo = new GameObject("AnimationOffset").transform;
            newGo.parent = go.parent;
            newGo.SetPositionAndRotation(go.position, go.rotation);
            go.parent = newGo;
            // quick and dirty
            go.localPosition = new Vector3(_offset * (1 - _axis.x), _offset * (1 - _axis.y), _offset*(1-_axis.z));
            go = newGo;
        }

        var clip = new AnimationClip();

        var start = _offset > 0?Vector3.zero: _startFromCurrentRotation ? go.localEulerAngles:Vector3.zero;
        var end = _startFromCurrentRotation ? new Vector3(start.x+359*Mathf.Clamp(_axis.x, -1, 1), start.y + 359 * Mathf.Clamp(_axis.y, -1, 1), start.z + 359 * Mathf.Clamp(_axis.z, -1, 1)) : new Vector3(359 * Mathf.Clamp(_axis.x, -1, 1), 359 * Mathf.Clamp(_axis.y, -1, 1), 359 * Mathf.Clamp(_axis.z, -1, 1));

        clip.SetCurve("", typeof(Transform), "localEulerAngles.x", AnimationCurve.Linear(0, start.x, 1/_animationSpeed, end.x));
        clip.SetCurve("", typeof(Transform), "localEulerAngles.y", AnimationCurve.Linear(0, start.y, 1/_animationSpeed, end.y));
        clip.SetCurve("", typeof(Transform), "localEulerAngles.z", AnimationCurve.Linear(0, start.z, 1/_animationSpeed, end.z));

        var relFolder = "Assets/" + _folder;
        if (!relFolder.EndsWith("/"))
        {
            relFolder += "/";
        }

        AssetDatabase.CreateAsset(clip, relFolder+filename);

        var controller =
            AnimatorController.CreateAnimatorControllerAtPathWithClip(relFolder + filename + ".controller", clip);

        go.gameObject.AddComponent<Animator>().runtimeAnimatorController = controller;

        Close();
    }

    private string IncrementalFilename(string folder, string filename)
    {
        if (!File.Exists(Path.Combine(folder, filename)))
        {
            return filename;
        }

        var extension = Path.GetExtension(filename);
        var filewoext = Path.GetFileNameWithoutExtension(filename);

        var currentIdx = 0;
        var newFilename = filewoext + currentIdx + extension;

        while (File.Exists(Path.Combine(folder, newFilename)))
        {
            currentIdx++;
            newFilename = filewoext + currentIdx + extension;
        }

        return newFilename;
    }

    public struct InitData
    {
        public GameObject GameObject;
    }
}