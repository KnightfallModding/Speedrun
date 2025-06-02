using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Il2CppInterop.Runtime.Injection;

namespace Speedrun;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BasePlugin
{
    internal static new ManualLogSource Log;
    public static ConfigEntry<bool> ENABLED;
    public static ConfigEntry<KeyCode> MAP_KEY;
    private static Harmony harmony;
    private static GameObject minimapChoiceGO;
    public static SpawnMap spawnMap;

    public override void Load()
    {
        // Initialize the config file. Needed first to be able to use the 'ENABLED' variable
        InitConfig();

        if (!ENABLED.Value)
            return;

        Log = base.Log;
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_NAME} loaded successfully!");
        
        // Register and instantiate the minimap canvas
        AddMinimap();

        // Harmony Patch
        harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll();
    }

    private void AddMinimap()
    {
        // Register custom class to Il2Cpp
        if (!ClassInjector.IsTypeRegisteredInIl2Cpp(typeof(SpawnMap)))
        {
            ClassInjector.RegisterTypeInIl2Cpp(typeof(SpawnMap));
        }

        // Create a new GameObject with the SpawnMapPlugin component
        minimapChoiceGO = new GameObject("MinimapChoice");
        spawnMap = minimapChoiceGO.AddComponent<SpawnMap>();

        // Add a Canvas component to the GameObject
        Canvas minimapChoiceCanvas = minimapChoiceGO.AddComponent<Canvas>();
        minimapChoiceCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        minimapChoiceCanvas.sortingOrder = 102; // Override black background from FadeCanvas with sortingOrder=100 & chatCanvas with sortingOrder=101        
        minimapChoiceGO.AddComponent<CanvasScaler>(); // needed ?
        minimapChoiceGO.AddComponent<GraphicRaycaster>(); // needed for click interactions
        minimapChoiceGO.AddComponent<CanvasGroup>(); // needed ?

        // Instantiate the canvas
        // Note: We don't use basePlugin.AddComponent<> because it will end up
        // on the same GameObject as all other basePlugin.AddComponent<> calls
        // And this object may already have a Canvas component present
        // This is for compatibility with other plugins
        minimapChoiceGO.hideFlags = HideFlags.HideAndDontSave;
    }

    private void InitConfig()
    {
        ENABLED = Config.Bind("General", "Enabled", true, "Enable or disable the Speedrun mod.");
        MAP_KEY = Config.Bind("Keybinds", "ToggleMapKey", KeyCode.M, "The key used to show or hide the spawn minimap.");
    }

    // May be useful for future use
    public override bool Unload()
    {
        ENABLED.Value = false; 

        // Remove the minimap object
        if (minimapChoiceGO != null)
        {
            GameObject.Destroy(minimapChoiceGO);
        }

        // Unpatch Harmony
        harmony.UnpatchSelf();

        return true; // Not sure what to return here
    }
}
