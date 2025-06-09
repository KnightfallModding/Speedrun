using Il2Cpp;
using System.IO;
using System.Linq;
using UnityEngine;
using MelonLoader;
using System.Reflection;

namespace Speedrun;

public class BundleHelper
{
    private const string BUNDLE_NAME = "knightfall.speedrun.bundle";
    private const string MINIMAP_ASSET_PATH = "Assets/Sprites/minimap-v2.jpg";
    private const string REFRESH_BUTTON_ASSET_PATH = "Assets/Sprites/refresh.png";
    private static Il2CppAssetBundle knightfallAssetBundle = null;

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
            Melon<Plugin>.Logger.Msg($"[BundleHelper] Loading {BUNDLE_NAME}...");
#endif
            var assembly = Assembly.GetExecutingAssembly();

            var fullResourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith(BUNDLE_NAME));

            if (fullResourceName == null)
                return null;

            using (var stream = assembly.GetManifestResourceStream(fullResourceName))
            {
                if (stream == null)
                {
                    Melon<Plugin>.Logger.Error($"An error occurred when loading bundle from {fullResourceName}. The bundle may be corrupted.");
                    return null;
                }

                using var memoryStream = new MemoryStream();
                stream.CopyTo(memoryStream);
                var bundleData = memoryStream.ToArray();

                knightfallAssetBundle = Il2CppAssetBundleManager.LoadFromMemory(bundleData);
            }

            return knightfallAssetBundle;
        }
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