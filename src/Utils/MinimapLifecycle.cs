using System;
using UnityEngine;
using UnityEngine.UI;
using Il2CppLandfall;

namespace Speedrun;

public class MinimapLifecycle
{
    public static SpawnMap spawnMap;
    public static ShowRecords showRecords;

    public static void AddMinimapAndRecords()
    {
        RecordsHandler.LoadRecords();

        // Create a new GameObject with the SpawnMap/ShowRecords components
        GameObject minimapChoiceGO = new GameObject("MinimapChoice");

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
        minimapChoiceCanvas.sortingOrder = 102;             // Override black background from FadeCanvas with
                                                            // sortingOrder=100 & chatCanvas with sortingOrder=101        
        minimapChoiceGO.AddComponent<CanvasScaler>();       // needed ?
        minimapChoiceGO.AddComponent<GraphicRaycaster>();   // needed for click interactions
        minimapChoiceGO.AddComponent<CanvasGroup>();        // needed ?

        showRecords.SetMinimapCanvas(minimapChoiceCanvas);

        DestroyMinimapOnGameStart();
    }

    /// <summary>
    /// Listener for `OnClick` event on the `Start` custom game button.
    /// Will automatically destroy the minimap objects when clicked.
    /// This should only be called in the `PhilipMenu` scene.
    /// </summary>
    public static void DestroyMinimapOnGameStart()
    {
        Button startButton = WaitingForPlayersUI._instance.m_StartButton;
        startButton.onClick.AddListener(new Action(DestroyMinimapAndRecords));
    }

    public static void DestroyMinimapAndRecords()
    {
        if (showRecords?.gameObject != null)
            GameObject.Destroy(showRecords.gameObject);

        if (spawnMap?.gameObject != null)
            GameObject.Destroy(spawnMap.gameObject);
    }
}