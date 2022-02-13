using UnityEditor;
using UnityEngine;

public static class PopupTools
{
    public static void Center(EditorWindow window, int width, int height)
    {
        var screenres = Screen.currentResolution;
        if (screenres.width == 3840)
        {
            screenres.height = 1440;
            screenres.width = 2560;
        }

        window.position = new Rect(screenres.width / 2f - width/2f, screenres.height / 2f -height/2f, width, height);
    }
}