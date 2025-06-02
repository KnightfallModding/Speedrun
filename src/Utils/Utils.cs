using System.IO;
using Photon.Pun;
using UnityEngine;
using UniverseLib;
using System;
using System.Linq;

namespace Speedrun;
public static class Utils
{
    private static AssetBundle knightfallAssetBundle = null;

    public static string GetFullAssetPath(string assetName)
    {
        string[] pathArray = { BepInEx.Paths.PluginPath, MyPluginInfo.PLUGIN_NAME, "assets", assetName };
        return Path.Combine(pathArray);
    }

    public static string GetKnightfallBundlePath()
    {
        return GetFullAssetPath("knightfall.speedrun.bundle");
    }

    public static AssetBundle LoadOrGetKnightfallBundle()
    {
        if (knightfallAssetBundle != null)
        {
            // Already loaded, return it directly
            return knightfallAssetBundle;
        }
        else
        {
            Plugin.Log.LogMessage($"Loading knightfall bundle...");

            string bundlePath = GetKnightfallBundlePath();
            if (!File.Exists(bundlePath))
            {
                Plugin.Log.LogError($"Failed to load bundle from {bundlePath}. Ensure the file exists.");
                return null;
            }

            knightfallAssetBundle = AssetBundle.LoadFromFile(bundlePath);
            if (knightfallAssetBundle == null)
            {
                Plugin.Log.LogError($"An error occurred when loading bundle {bundlePath}. The bundle may be corrupted.");
            }

            return knightfallAssetBundle;
        }
    }

    public static bool IsCustomGame()
    {
        // Custom game must NOT have more than 2 players (one team).
        // To allow more than 2 players, we would have to ensure multiple players
        // selecting the same spawn point will not result in conflicts.
        // Thus, this will probably never be added as a feature.
        bool inQueueOrInGame = PhotonNetwork.InRoom;
        if (!inQueueOrInGame)
            return false;
            
        bool max2Players = PhotonNetwork.CurrentRoom.PlayerCount <= 2;
        bool isCustom = PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("CustomGame");
        
        return inQueueOrInGame && max2Players && isCustom;
    }

    public static bool HasGameStarted()
    {
        return GM_Adventure.instance != null && GM_Adventure.instance.gameHasStarted;
    }

    public static Sprite LoadMinimapSprite()
    {
        AssetBundle assetBundle = Utils.LoadOrGetKnightfallBundle();
        if (assetBundle == null)
            return null;

        Sprite minimapSprite = null;
        Texture2D texture = assetBundle.LoadAsset<Texture2D>("Assets/Sprites/minimap-v2.jpg");
        if (texture != null)
        {
            // Convert the Texture2D to a Sprite (if necessary for UI)
            minimapSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }

        return minimapSprite;
    }

    // Helper method to parse a comma-separated string into a float array
    // for the TimeRecords config
    public static float[] GetRecordsList()
    {
        try
        {
            string value = Plugin.TIME_RECORDS.Value;
            return value.Split(';').Select(float.Parse).ToArray();
        }
        catch (Exception ex)
        {
            Plugin.Log.LogError($"GetRecordsList -> Failed to parse float list: {ex.Message}");
            return [-1,-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1]; // Return default if parsing fails
        }
    }

    // Helper method to save a float array back to the config
    public static void SaveRecords(float[] recordsList)
    {
        Plugin.TIME_RECORDS.Value = string.Join(";", recordsList);
    }
}
