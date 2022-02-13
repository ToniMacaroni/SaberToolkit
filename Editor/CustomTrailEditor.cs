using CustomSaber;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CustomTrail))]
[CanEditMultipleObjects]
public class CustomTrailEditor : Editor
{

    void OnEnable()
    {
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawPropertiesExcluding(serializedObject, "m_Script");
        serializedObject.ApplyModifiedProperties();
    }

    [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected)]
    private static void DrawGizmo(CustomTrail trail, GizmoType gizmoType)
    {
        if (!SaberTools.Instance || !SaberTools.Instance.ShowTrailPreview)
        {
            return;
        }

        var mat = trail.TrailMaterial;
        if (!mat)
        {
            return;
        }

        var pStartPosition = trail.PointStart.localPosition;
        var pEndPosition = trail.PointEnd.localPosition;

        var mesh = new Mesh();
        mesh.name = "TrailPreviewMesh";

        var offsetVec = new Vector3(SaberTools.Instance?SaberTools.Instance.TrailPreviewLength:0.5f, 0, 0);

        mesh.vertices = new[]
        {
            pStartPosition,
            pStartPosition+offsetVec,
            pEndPosition,
            pEndPosition+offsetVec
        };


        mesh.uv = new[]
        {
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 0),
            new Vector2(0, 1)
        };

        if (SaberTools.Instance && SaberTools.Instance.PreviewCC)
        {
            var color = trail.name == "LeftSaber" ? SaberTools.Instance.CustomColorLeft : SaberTools.Instance.CustomColorRight;
            mesh.colors = new[] { color, color, color, color };
        }

        int[] triangles = new int[6];
        for (int ti = 0, vi = 0, y = 0; y < 1; y++, vi++)
        {
            for (int x = 0; x < 1; x++, ti += 6, vi++)
            {
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + 1 + 1;
                triangles[ti + 5] = vi + 1 + 2;
            }
        }
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        mat.SetPass(0);
        Graphics.DrawMeshNow(mesh, trail.PointStart.parent.localToWorldMatrix);
    }
}