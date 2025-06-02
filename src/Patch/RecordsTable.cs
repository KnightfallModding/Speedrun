using Il2Cpp;
using HarmonyLib;

namespace Speedrun;

public class RecordsTable
{
    /// <summary>
    /// Refresh records table when the user goes back to the lobby after a game.
    /// </summary>
    [HarmonyPatch(typeof(PhotonServerConnector), nameof(PhotonServerConnector.Start))]
    public class PSC_Patch
    {
        [HarmonyPostfix]
        static void PSC_Postfix()
        {
            Plugin.showRecords.UpdateRecordsTables();
        }
    }
}