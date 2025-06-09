using Il2Cpp;
using System.IO;
using Il2CppPhoton.Pun;
using MelonLoader.Utils;

namespace Speedrun;

public static class Utils
{
    public static string GetFullAssetPath(string assetName)
    {
        string[] pathArray = [MelonEnvironment.ModsDirectory, ModInfo.MOD_NAME, "assets", assetName];
        return Path.Combine(pathArray);
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
}
