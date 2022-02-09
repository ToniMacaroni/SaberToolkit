using System;
using UnityEditor;
using UnityEngine;

public class BaseTheme : ScriptableObject
{
    private const string ThemeBasePath = "Packages/com.factorycore.tonimacaroni.sabertoolkit/PersistentData/";

    public string Name;

    public Color BackgroundColor = Color.white;
    public Color HeaderColor = Color.black;
    public Color WarningColor = Color.yellow;
    public Color ErrorColor = Color.red;
    public Color SuccessColor = Color.green;

    private static BaseTheme _instance;

    public static BaseTheme GetTheme()
    {
        if (!_instance || !IsCorrectSkin(_instance))
        {
            var filename = ThemeBasePath + (EditorGUIUtility.isProSkin ? "DarkTheme.asset" : "LightTheme.asset");
            var theme = AssetDatabase.LoadAssetAtPath<BaseTheme>(filename);
            if (!theme)
            {
                Debug.LogWarning($"Failed to load theme: {filename}");
                theme = CreateInstance<BaseTheme>();
                AssetDatabase.CreateAsset(theme, filename);
                AssetDatabase.SaveAssets();
            }
            _instance = theme;
        }

        return _instance;
    }

    private static bool IsCorrectSkin(BaseTheme theme)
    {
        var isDark = EditorGUIUtility.isProSkin;
        return (theme.Name == "Light" && !isDark) || (theme.Name == "Dark" && isDark);
    }
}