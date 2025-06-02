using Il2Cpp;
using System.IO;
using Il2CppPhoton.Pun;
using UnityEngine;
using System.Linq;
using MelonLoader;
using MelonLoader.Utils;

namespace Speedrun;

public static class Utils
{
    private static Il2CppAssetBundle knightfallAssetBundle = null;

    public static string GetFullAssetPath(string assetName)
    {
        string[] pathArray = [MelonEnvironment.ModsDirectory, ModInfo.MOD_NAME, "assets", assetName];
        return Path.Combine(pathArray);
    }

    public static string GetKnightfallBundlePath()
    {
        return GetFullAssetPath("knightfall.speedrun.bundle");
    }

    public static Il2CppAssetBundle LoadOrGetKnightfallBundle()
    {
        if (knightfallAssetBundle != null)
        {
            // Already loaded, return it directly
            return knightfallAssetBundle;
        }
        else
        {
            Melon<Plugin>.Logger.Msg($"Loading knightfall bundle...");

            string bundlePath = GetKnightfallBundlePath();
            if (!File.Exists(bundlePath))
            {
                Melon<Plugin>.Logger.Error($"Failed to load bundle from {bundlePath}. Ensure the file exists.");
                return null;
            }

            knightfallAssetBundle = Il2CppAssetBundleManager.LoadFromFile(bundlePath);
            if (knightfallAssetBundle == null)
            {
                Melon<Plugin>.Logger.Error($"An error occurred when loading bundle {bundlePath}. The bundle may be corrupted.");
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
        Il2CppAssetBundle assetBundle = LoadOrGetKnightfallBundle();
        if (assetBundle == null)
            return null;

        Sprite minimapSprite = null;
        Texture2D texture = assetBundle.LoadAsset<Texture2D>("Assets/Sprites/minimap-v2.jpg");
        if (texture != null)
        {
            // Convert the Texture2D to a Sprite
            minimapSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }

        return minimapSprite;
    }

    public static float[] GetDefaultRecordsList() => [.. Enumerable.Repeat(-1f, 14)];
    public static float[] GetRecordsList() => Plugin.TIME_RECORDS.Value;
    public static void SaveRecords(float[] recordsList) => Plugin.TIME_RECORDS.Value = recordsList;
}
