using Speedrun;
using UnityEngine;
using MelonLoader;
using Il2CppInterop.Runtime.Injection;

[assembly: MelonInfo(typeof(Plugin), ModInfo.MOD_NAME, ModInfo.MOD_VERSION, ModInfo.MOD_AUTHOR, $"{ModInfo.MOD_LINK}/releases/latest/download/Release.zip")]
[assembly: MelonGame("Landfall Games", "Knightfall")]

namespace Speedrun;

internal class Plugin : MelonMod
{
    public static MelonPreferences_Entry<bool> ENABLED;
    public static MelonPreferences_Entry<KeyCode> MAP_KEY;

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
    }

    private void InitConfig()
    {
        var mlCategory = MelonPreferences.CreateCategory("Speedrun", "Speedrun settings");

        ENABLED = mlCategory.CreateEntry("Enabled", true);
        MAP_KEY = mlCategory.CreateEntry("ToggleMapKey", KeyCode.M);
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

    /// <summary>
    /// Remove all Harmony Patches.
    /// </summary>
    private void UnloadMod() => HarmonyInstance.UnpatchSelf();
}
