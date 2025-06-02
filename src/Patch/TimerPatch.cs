using Il2Cpp;
using HarmonyLib;
using MelonLoader;
using UnityEngine;
using Il2CppTMPro;
using UnityEngine.UI;

namespace Speedrun;

public class TimerPatch
{
    private static Timer timer;
    private static string CASTLE_TOWN_NAME = "ROSE CASTLE";

    [HarmonyPatch(typeof(GM_Adventure), nameof(GM_Adventure.RPCA_StartGame))]
    public class StartGamePatch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            if (!Plugin.ENABLED.Value || !Utils.IsCustomGame())
                return;

            GameObject timerBackgroundGO = new GameObject("SpeedrunTimerBackground");

            // Add Image component to the background
            var backgroundImage = timerBackgroundGO.AddComponent<Image>();
            backgroundImage.color = new Color(0, 0, 0, 0.80f);

            // Create new gameObject with this component
            // and add it as a child of the main canvas
            GameObject timerGO = new GameObject("SpeedrunTimer");
            timer = timerGO.AddComponent<Timer>();

            var timerText = timerGO.AddComponent<TextMeshProUGUI>();
            timerText.alignment = TextAlignmentOptions.Center;
            timerText.color = Color.white;

            foreach (var obj in new GameObject[2] { timerBackgroundGO, timerGO })
            {
                obj.transform.SetParent(MainCanvas.instance.transform);

                // Top-right corner
                obj.GetComponent<RectTransform>().anchorMin = new Vector2(1, 1);
                obj.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);

                // Align the text to the right
                obj.GetComponent<RectTransform>().pivot = new Vector2(1, 1);
                obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(-50, 0);
            }

            timerGO.transform.SetParent(timerBackgroundGO.transform);

            // Automatically resize the background to fit the text
            timerGO.GetComponent<RectTransform>().anchoredPosition += new Vector2(0, -10); // center a bit better
            timerText.text = timer.GetTimerFullText();
            backgroundImage.rectTransform.sizeDelta = timerText.rectTransform.sizeDelta + new Vector2(0, 20);

            backgroundImage.rectTransform.anchoredPosition = new Vector2(-25, 0);

            // Enable Castle entry collider because it's normally disabled until 3rd day
            EnableCastleEntryCollider();
        }
    }

    [HarmonyPatch(typeof(HeaderHandler), nameof(HeaderHandler.PlayerHeader))]
    [HarmonyPatch([typeof(HeaderType), typeof(string)])]
    public class SpeedrunEndingPatch
    {
        [HarmonyPostfix]
        public static void Postfix(HeaderHandler __instance, HeaderType headerType, string townName)
        {
            if (!Plugin.ENABLED.Value || !Utils.IsCustomGame())
                return;

            // Only stop if we reached castle
            if (headerType == HeaderType.EnterTown_Occupied || headerType == HeaderType.EnterTown_Empty)
            {
                if (townName == CASTLE_TOWN_NAME)
                {
                    // Stop the timer when the game ends
                    timer.StopTimer();

                    // Change the notification text to show the time it took to reach the Castle
                    // Note: we don't use a Prefix with return false.
                    // We just override the original notification (that is already playing since we're in postfix)
                    // with our new notification
                    float finalTime = timer.GetTime();
                    string finalTimeStr = timer.GetFormattedTime();
                    __instance.headerText.text = $"Time to Castle: <color=red>{finalTimeStr}</color>";
                    __instance.subtitleText.text = "";

                    // Check if it's a record and update the records if necessary
                    int spawnPointIndex = SpawnMap.shownSpawnPoint - 1; // -1 to start at index 0
                    bool isRecord = RecordsHandler.IsRecord(spawnPointIndex, finalTime);

                    // Display 'New record' notification if it's a record
                    if (isRecord)
                    {
                        Melon<Plugin>.Logger.Msg($"New record for spawn {spawnPointIndex + 1}! Time: {finalTime}s");
                        __instance.subtitleText.text = "<color=green>New record!</color>";

                        RecordsHandler.UpdateRecord(spawnPointIndex, finalTime);
                    }
                    // Small easter egg for fast players
                    else if (finalTime < 120)
                    {
                        __instance.subtitleText.text = "Bro really is speedrunning <sprite=3>";
                    }

                    timer.hasFinished = true;
                }
            }
        }
    }

    private static void EnableCastleEntryCollider() => GameObject.Find("EndCastle").GetComponent<Farm>().col.enabled = true;
}