using System;
using HarmonyLib;
using Landfall.Network;

namespace Speedrun;

public class RecordsTable
{
    // Refresh records table when the user goes back to the lobby after a game
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