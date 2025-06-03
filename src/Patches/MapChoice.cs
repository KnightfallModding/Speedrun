using Il2Cpp;
using HarmonyLib;
using Il2CppPhoton.Pun;

namespace Speedrun;

[HarmonyPatch]
public class MapChoice
{
    [HarmonyPatch(typeof(GM_Lobby), nameof(GM_Lobby.Start))]
    [HarmonyPostfix]
    static void GM_LobbyStartPatch()
    {
        // Plugin is either disabled, or we are not in a custom game
        if (!Plugin.ENABLED.Value || !Utils.IsCustomGame())
            return;

        if (PhotonNetwork.IsMasterClient)
            MinimapLifecycle.AddMinimapAndRecords();
    }

    [HarmonyPatch(typeof(GM_Adventure), nameof(GM_Adventure.InitPlayer))]
    [HarmonyPrefix]
    static void InitPlayerPatch(ref int i, int j, Team currTeam)
    {
        // Plugin is either disabled, or we are not in a custom game
        if (!Plugin.ENABLED.Value || !Utils.IsCustomGame())
            return;

        // Modify the spawn point of our team to the chosen spawn point
        if (currTeam.TeamID == Player.localplayer.TeamID)
            i = SpawnHandler.chosenSpawnPoint;
    }
}
