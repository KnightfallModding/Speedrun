using Il2Cpp;
using System.IO;
using UnityEngine;
using MelonLoader;
using Il2CppPhoton.Pun;
using MelonLoader.Utils;

namespace Speedrun;

public static class Utils
{
    private const string BUNDLE_NAME = "knightfall.speedrun.bundle";
    private const string MINIMAP_ASSET_PATH = "Assets/Sprites/minimap-v2.jpg";
    private const string REFRESH_BUTTON_ASSET_PATH = "Assets/Sprites/refresh.png";

    private static Il2CppAssetBundle knightfallAssetBundle = null;

    public static string GetFullAssetPath(string assetName)
    {
        string[] pathArray = [MelonEnvironment.ModsDirectory, ModInfo.MOD_NAME, "assets", assetName];
        return Path.Combine(pathArray);
    }

    public static string GetKnightfallBundlePath()
    {
        return GetFullAssetPath(BUNDLE_NAME);
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
#if DEBUG
            Melon<Plugin>.Logger.Msg($"Loading {BUNDLE_NAME} bundle...");
#endif

            string bundlePath = GetKnightfallBundlePath();
            if (!File.Exists(bundlePath))
            {
                Melon<Plugin>.Logger.Error($"Failed to load bundle from {bundlePath}. Ensure the file exists.");
                return null;
            }

            knightfallAssetBundle = Il2CppAssetBundleManager.LoadFromFile(bundlePath);
            if (knightfallAssetBundle == null)
            {
                Melon<Plugin>.Logger.Error($"An error occurred when loading bundle from {bundlePath}. The bundle may be corrupted.");
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
        Texture2D texture = assetBundle.LoadAsset<Texture2D>(MINIMAP_ASSET_PATH);

        // Convert the Texture2D to a Sprite
        if (texture != null)
            minimapSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

        return minimapSprite;
    }

    public static Sprite LoadResetButtonSprite()
    {
        Il2CppAssetBundle assetBundle = LoadOrGetKnightfallBundle();
        if (assetBundle == null)
            return null;

        Sprite resetBtnSprite = null;
        Texture2D texture = assetBundle.LoadAsset<Texture2D>(REFRESH_BUTTON_ASSET_PATH);

        // Convert the Texture2D to a Sprite
        if (texture != null)
            resetBtnSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

        return resetBtnSprite;
    }
}
