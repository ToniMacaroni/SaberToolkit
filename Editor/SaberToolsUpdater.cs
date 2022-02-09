using System;
using System.Net;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

[InitializeOnLoad]
internal class SaberToolsUpdater
{
    public static Version RemoteVersion;
    public static Version LocalVersion;

    public static string RemoteVersionString = "0.0.0";
    public static string LocalVersionString = "0.0.0";

    public static bool IsUpdateAvailable;
    public static bool IsUpdating;

    static SaberToolsUpdater()
    {
        _ = CheckForUpdate();
    }

    public static async Task CheckForUpdate()
    {
        try
        {
            var packageStr = AssetDatabase.LoadAssetAtPath<TextAsset>("Packages/com.tonimacaroni.sabertoolkit/package.json");
            var localData = JsonUtility.FromJson<PackageInfo>(packageStr.text);
            LocalVersion = Version.Parse(localData.version);
            LocalVersionString = LocalVersion.ToString();

            var client = new WebClient();
            var content =
                await client.DownloadStringTaskAsync(
                    "https://raw.githubusercontent.com/ToniMacaroni/SaberToolkit/main/package.json");
            var data = JsonUtility.FromJson<PackageInfo>(content);
            RemoteVersion = Version.Parse(data.version);
            RemoteVersionString = RemoteVersion.ToString();

            IsUpdateAvailable = LocalVersion.CompareTo(RemoteVersion) < 0;
        }
        catch (Exception e)
        {
            Debug.Log(e);
            Debug.LogWarning("Couldn't check for SaberTools update");
        }
    }

    public static async Task Update(bool force = false)
    {
        if (!force && (!IsUpdateAvailable || IsUpdating))
        {
            return;
        }

        IsUpdating = true;
        Debug.Log("Updating ...");
        var addReq = Client.Add("https://github.com/ToniMacaroni/SaberToolkit.git");
        while (!addReq.IsCompleted)
        {
            await Task.Delay(100);
        }

        Debug.Log("Completed");
        Debug.Log("Update " + (addReq.Status==StatusCode.Success?"Success":"Failure"));

        await Task.Delay(3000);

        IsUpdating = false;
        await CheckForUpdate();
    }

    internal class PackageInfo
    {
        public string version;
    }
}