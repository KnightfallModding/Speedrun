#if DEBUG
using Il2Cpp;
using Il2CppPhoton.Pun;
using HarmonyLib;

namespace Speedrun;

[HarmonyPatch]
public class FakeGrassPatch
{
    [HarmonyPatch(typeof(Horse), nameof(Horse.GetRoadMultiplier))]
    [HarmonyPostfix]
    public static void GetRoadMultiplierPatch()
    {
    }
}
#endif