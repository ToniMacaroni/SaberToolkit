using System.IO;
using UnityEditor;
using UnityEngine;

public class SaberProjectSettings : ScriptableObject
{
    public const string SaberProjectDomain = "com.factorycore.tonimacaroni.sabertoolkit";
    public const string NpmRegistryHost = "http://195.90.208.226:4874";
    public const string SettingsPath = "Assets/_SaberToolkitData/SaberProjectSettings.asset";

    [SerializeField] public string BeatSaberPath;
    [SerializeField] public string Author;
    [SerializeField] public bool CreateRightSaberOnExport = true;
    [SerializeField] public string ExportFilename = "{SaberName}.saber";
    [SerializeField] public bool ShowOverlay = true;

    [SerializeField] public string SteamPath = ".../.../steam.exe";
    [SerializeField] public string AdditionalLaunchArguments = "-vrmode oculus --verbose fpfc";

    internal bool IsPathValid()
    {
        return !string.IsNullOrWhiteSpace(BeatSaberPath) && File.Exists(Path.Combine(BeatSaberPath, "Beat Saber.exe"));
    }

    internal bool IsSteamPathValid()
    {
        return !string.IsNullOrWhiteSpace(SteamPath) && File.Exists(SteamPath);
    }

    internal static SaberProjectSettings GetOrCreateSettings()
    {
        var settings = AssetDatabase.LoadAssetAtPath<SaberProjectSettings>(SettingsPath);
        if (settings == null)
        {
            Directory.CreateDirectory(Path.Combine(Application.dataPath, SettingsPath.Substring(7)));
            settings = CreateInstance<SaberProjectSettings>();
            AssetDatabase.CreateAsset(settings, SettingsPath);
            AssetDatabase.SaveAssets();
        }
        return settings;
    }

    internal static SerializedObject GetSerializedSettings()
    {
        return new SerializedObject(GetOrCreateSettings());
    }

    internal static void OpenSettingsScreen()
    {
        SettingsService.OpenProjectSettings("Project/Saber Exporter");
    }
}