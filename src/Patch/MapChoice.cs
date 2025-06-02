using HarmonyLib;
using Landfall.Network;

namespace Speedrun;

public class MapChoice
{
    [HarmonyPatch(typeof(CustomGameConnector), nameof(CustomGameConnector.HostCustomGame))]
    public class HostCustomGamePatch
    {
        [HarmonyPostfix]
        static void HostCustomGame()
        {
            if (Plugin.ENABLED.Value)
            {
                Plugin.spawnMap.ShowMap();
            }
        }
    }

    [HarmonyPatch(typeof(GM_Adventure), nameof(GM_Adventure.InitPlayer))]
    public class InitPlayerPatch
    {
        [HarmonyPrefix]
        static void InitPlayerPrefix(ref int i, int j, Team currTeam)
        {
            // Plugin is either disabled, or we are not in a custom game
            // or we did not choose a specific spawn point
            if (!Plugin.ENABLED.Value || !Utils.IsCustomGame())
                return;

            // If we did actually choose a specific spawn point
            // Move all players of our team to the chosen spawn point
            if (SpawnMap.chosenSpawnPoint != -1 && currTeam.TeamID == Player.localplayer.TeamID)
            {
                i = SpawnMap.chosenSpawnPoint;
            }

            // In any case, disable the minimap choice canvas here
            Plugin.spawnMap.HideMap();
        }
    }

    [HarmonyPatch(typeof(PhotonServerConnector), nameof(PhotonServerConnector.Start))]
    public class PSC_Patch
    {
        [HarmonyPostfix]
        static void PSC_Postfix()
        {
            // Just make sure the map is closed when the user joins the lobby
            // This is to prevent the map from staying open after a custom game
            // for example
            Plugin.spawnMap.HideMap();
        }
    }
}
