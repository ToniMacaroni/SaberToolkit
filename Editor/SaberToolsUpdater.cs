using System;
using System.Collections.Generic;
using System.Linq;
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

    public static async Task CheckForUpdate(int numTries = 0)
    {
        if (numTries > 2)
        {
            Debug.LogWarning("Giving up on checking for SaberToolkit update");
            return;
        }

        try
        {
            var package = await GetLocalPackage();
            if (package != null)
            {
                LocalVersion = Version.Parse(package.version);
                LocalVersionString = package.version;
                Debug.Log($"Got local {LocalVersionString}");
            }

            var remotePackage = await GetRemotePackage();
            if (remotePackage != null)
            {
                RemoteVersion = Version.Parse(remotePackage.version);
                RemoteVersionString = RemoteVersion.ToString();
                Debug.Log($"Got remote {RemoteVersionString}");
            }

            if (LocalVersion != null && RemoteVersion != null)
            {
                IsUpdateAvailable = LocalVersion.CompareTo(RemoteVersion) < 0;
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
            Debug.LogWarning("Couldn't check for SaberToolkit update\nTrying again");
            await Task.Delay(1000);
            await CheckForUpdate(numTries + 1);
        }
    }

    public static async Task<UnityEditor.PackageManager.PackageInfo> GetRemotePackage()
    {
        var seRe = Client.Search(SaberProjectSettings.SaberProjectDomain);
        while (!seRe.IsCompleted)
        {
            await Task.Delay(100);
        }

        if (seRe.Status == StatusCode.Failure)
        {
            Debug.LogWarning("Error getting sabertoolkit package");
            Debug.LogWarning(seRe.Error.message);
            return null;
        }

        return seRe.Result[0];
    }

    public static async Task<UnityEditor.PackageManager.PackageInfo> GetLocalPackage()
    {
        var liRe = Client.List();
        while (!liRe.IsCompleted)
        {
            await Task.Delay(100);
        }

        if (liRe.Status == StatusCode.Failure)
        {
            Debug.LogWarning("Error getting sabertoolkit package");
            Debug.LogWarning(liRe.Error.message);
            return null;
        }

        var package = liRe.Result.First(x => x.name == SaberProjectSettings.SaberProjectDomain);


        return package;
    }

    public static async Task Update(bool force = false)
    {
        if (!force && (!IsUpdateAvailable || IsUpdating))
        {
            return;
        }
        IsUpdating = true;
        Debug.Log("Updating ...");
        var addReq = Client.Add(SaberProjectSettings.SaberProjectDomain);
        while (!addReq.IsCompleted)
        {
            await Task.Delay(100);
        }

        Debug.Log("Completed");

        if (addReq.Status == StatusCode.Failure)
        {
            Debug.LogWarning(addReq.Error.message);
        }

        Debug.Log("Update " + (addReq.Status==StatusCode.Success?"Success":"Failure"));

        await Task.Delay(3000);

        IsUpdating = false;
        await CheckForUpdate();
    }
}