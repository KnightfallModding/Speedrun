using UnityEngine;
using UnityEngine.UI;
using MelonLoader;
using Il2CppInterop.Runtime.Injection;
using Speedrun;

[assembly: MelonInfo(typeof(Plugin), ModInfo.MOD_NAME, ModInfo.MOD_VERSION, ModInfo.MOD_AUTHOR, $"{ModInfo.MOD_LINK}/releases/latest/download/Release.zip")]
[assembly: MelonGame("Landfall Games", "Knightfall")]

namespace Speedrun;

internal class Plugin : MelonMod
{
    public static MelonPreferences_Entry<bool> ENABLED;
    public static MelonPreferences_Entry<KeyCode> MAP_KEY;
    public static MelonPreferences_Entry<float[]> TIME_RECORDS;
    private static GameObject minimapChoiceGO;
    public static SpawnMap spawnMap;
    public static ShowRecords showRecords;

    public override void OnInitializeMelon()
    {
        InitConfig();

        if (!ENABLED.Value)
        {
            UnloadMod();
            return;
        }

        LoggerInstance.Msg($"Plugin {ModInfo.MOD_NAME} loaded successfully!");

        RegisterComponents();
        AddMinimapAndRecords();
    }

    private void InitConfig()
    {
        var mlCategory = MelonPreferences.CreateCategory("Speedrun", "Speedrun settings");

        ENABLED = mlCategory.CreateEntry("Enabled", true);
        MAP_KEY = mlCategory.CreateEntry("ToggleMapKey", KeyCode.M);
        TIME_RECORDS = mlCategory.CreateEntry("TimeRecords", Utils.GetDefaultRecordsList());

        // Remove all Harmony Patches
        if (!ENABLED.Value)
            UnloadMod();
    }

    /// <summary>
    /// Register custom classes to Il2Cpp.
    /// </summary>
    private static void RegisterComponents()
    {
        if (!ClassInjector.IsTypeRegisteredInIl2Cpp(typeof(SpawnMap)))
            ClassInjector.RegisterTypeInIl2Cpp(typeof(SpawnMap));

        if (!ClassInjector.IsTypeRegisteredInIl2Cpp(typeof(ShowRecords)))
            ClassInjector.RegisterTypeInIl2Cpp(typeof(ShowRecords));

        if (!ClassInjector.IsTypeRegisteredInIl2Cpp(typeof(Timer)))
            ClassInjector.RegisterTypeInIl2Cpp(typeof(Timer));
    }

    private static void AddMinimapAndRecords()
    {
        // Create a new GameObject with the SpawnMapPlugin component
        minimapChoiceGO = new GameObject("MinimapChoice");
        showRecords = minimapChoiceGO.AddComponent<ShowRecords>();
        spawnMap = minimapChoiceGO.AddComponent<SpawnMap>();

        // Add a Canvas component to the GameObject
        // Note: We don't use basePlugin.AddComponent<> because it will end up
        // on the same GameObject as all other basePlugin.AddComponent<> calls
        // This object may already have a Canvas component present, which will
        // cause an exception.
        // This is for compatibility with other plugins.
        Canvas minimapChoiceCanvas = minimapChoiceGO.AddComponent<Canvas>();
        minimapChoiceCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        minimapChoiceCanvas.sortingOrder = 102; // Override black background from FadeCanvas with sortingOrder=100 & chatCanvas with sortingOrder=101        
        minimapChoiceGO.AddComponent<CanvasScaler>(); // needed ?
        minimapChoiceGO.AddComponent<GraphicRaycaster>(); // needed for click interactions
        minimapChoiceGO.AddComponent<CanvasGroup>(); // needed ?
        minimapChoiceGO.hideFlags = HideFlags.HideAndDontSave;
    }

    /// <summary>
    /// Remove all Harmony Patches.
    /// </summary>
    private void UnloadMod() => HarmonyInstance.UnpatchSelf();
}
