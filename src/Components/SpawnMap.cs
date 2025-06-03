using System;
using MelonLoader;
using UnityEngine;
using Il2CppTMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace Speedrun;

public class SpawnMap : MonoBehaviour
{
    private readonly Vector2[] spawnPointCoordInJPG =
    [
            new(40, 1640),   // 1 - added X offset by hand
            new(218, 1640),
            new(139, 1446),
            new(369, 1590),  // 4
            new(568, 1445),
            new(817, 1485),
            new(925, 1615),
            new(1130, 1460), // 8
            new(1294, 1595),
            new(1361, 1471), // 10
            new(1680, 1332),
            new(1748, 800),  // 12 - added Y offset by hand
            new(1485, 350),  // 13 - added Y offset by hand
            new(1624, 180)   // 14 - added Y offset by hand
    ];

    // Actual spawn order based on the map from discord
    //
    // Spawn numbers are based off the map
    // List order is based on `RPCA_InitPlayer(int spawnID)`
    //
    // This means that the first spawn of the list will be the one you spawn to
    // when calling `RPCA_InitPlayer(0)` and the last one for `RPCA_InitPlayer(13)`
    //
    // Spawn order: 1, 2, 4, 6, 7, 9, 10, 11, 12, 13, 14, 3, 8, 5
    private readonly int[] spawnPointOrderMapping = // I won't even explain how I obtained these... xd
    [
        0,
        1,
        11,
        2,
        13,
        3,
        4,
        12,
        5,
        6,
        7,
        8,
        9,
        10
    ];

    // NOTE: Do NOT change this.
    // Minimap coords are supposed to be dynamic but for some reason
    // it does not work at all with other values than (500, 500).
    private Vector2 minimapSize = new(500, 500);
    private Vector2 spawnButtonSize = new(30, 30);
    private TextMeshProUGUI choiceTMPro;
    private List<Image> buttonImages = new();

    public void Start()
    {
        // 1. Load minimap image & find canvas
        Sprite minimapSprite = Utils.LoadMinimapSprite();

        Melon<Plugin>.Logger.Msg($"Creating minimap with choices...");

        Canvas minimapChoiceCanvas = this.gameObject.GetComponent<Canvas>();

        // 2. Create an Image UI element to display the minimap
        GameObject minimapObject = new("MinimapChoiceImage");
        minimapObject.transform.SetParent(minimapChoiceCanvas.transform);

        // Add Image component to display the minimap sprite
        Image minimapImage = minimapObject.AddComponent<Image>();
        minimapImage.sprite = minimapSprite;

        // 3. Set the RectTransform to anchor the minimap to the bottom-right corner
        RectTransform rectTransform = minimapObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(1, 0); // Bottom-Right anchor
        rectTransform.anchorMax = new Vector2(1, 0); // Bottom-Right anchor
        rectTransform.pivot = new Vector2(1, 0);     // Pivot also in bottom-right corner

        // the fuck xd
        int originalWidth = 1853;
        int originalHeight = 1707;

        Vector2 canvasOffset = new(-20, 30);         // 20 pixels from bottom and 30 for right
        Vector2 canvasSize = minimapSize;

        // Calculate the scale multiplier based on the original image dimensions and the canvas size
        float scaleMultiplierX = canvasSize.x / originalWidth;
        float scaleMultiplierY = canvasSize.y / originalHeight;

        // Use the smaller scale to ensure the aspect ratio is preserved
        float scaleFactor = Mathf.Min(scaleMultiplierX, scaleMultiplierY);

        // Set the position and size of the minimap
        rectTransform.anchoredPosition = canvasOffset;
        rectTransform.sizeDelta = new Vector2(500, 500);

        // Add a button on each spawn point of the map
        int i = 0;
        foreach (Vector2 jpgPoint in spawnPointCoordInJPG)
        {
            // Scale the points based on the scale factor
            float scaledX = jpgPoint.x * scaleFactor;
            float scaledY = jpgPoint.y * scaleFactor;

            // Create a button at the calculated position
            GameObject buttonObject = new($"SpawnPointButton{i}");
            buttonObject.transform.SetParent(minimapChoiceCanvas.transform);

            // Add Button and Image components to the button
            Button spawnButton = buttonObject.AddComponent<Button>();
            Image spawnButtonImage = buttonObject.AddComponent<Image>();
            buttonImages.Add(spawnButtonImage);

            // Set a simple color (could be image too) for the button
#if DEBUG
            spawnButtonImage.color = new Color(1, 0, 0, 0.6f);
#else
            spawnButtonImage.color = Color.clear; // transparent button
#endif

            float finalX = (scaledX + canvasOffset.x) - canvasSize.x;
            float finalY = canvasSize.y - (scaledY); // Adjust the Y axis to match Unity's coordinate system (bottom-left origin)

            Vector2 buttonPosition = new(finalX, finalY);

            RectTransform buttonRectTransform = buttonObject.GetComponent<RectTransform>();
            buttonRectTransform.sizeDelta = spawnButtonSize;
            buttonRectTransform.anchorMin = new Vector2(1, 0);     // Bottom-right of the canvas
            buttonRectTransform.anchorMax = new Vector2(1, 0);     // Bottom-right of the canvas
            buttonRectTransform.pivot = new Vector2(0.5f, 0.5f);   // Center the pivot
            buttonRectTransform.anchoredPosition = buttonPosition; // Positions

            int spawnIndex = i; // Capture the index to use in the click handler
            spawnButton.onClick.AddListener(new Action(() => OnButtonClicked(spawnIndex)));

            // Add an EventTrigger component to the button
            EventTrigger trigger = buttonObject.AddComponent<EventTrigger>();

            // Set up OnPointerEnter event
            EventTrigger.Entry enterEntry = new();
            enterEntry.eventID = EventTriggerType.PointerEnter;
            enterEntry.callback.AddListener((UnityAction<BaseEventData>)((eventData) => { OnPointerEnter(spawnIndex); }));
            trigger.triggers.Add(enterEntry);

            // Set up OnPointerExit event
            EventTrigger.Entry exitEntry = new();
            exitEntry.eventID = EventTriggerType.PointerExit;
            exitEntry.callback.AddListener((UnityAction<BaseEventData>)((eventData) => { OnPointerExit(spawnIndex); }));
            trigger.triggers.Add(exitEntry);

            i++;
        }

        // Add a button to reset the current choice to 'random'
        GameObject resetButtonObject = new($"ResetChoiceButton");
        Button resetButton = resetButtonObject.AddComponent<Button>();
        Image resetButtonImage = resetButtonObject.AddComponent<Image>();
        resetButtonObject.transform.SetParent(minimapChoiceCanvas.transform);

        Sprite resetButtonSprite = Utils.LoadResetButtonSprite();
        resetButtonImage.sprite = resetButtonSprite;

        // Set refresh button pos
        RectTransform resetButtonRectTransform = resetButtonObject.GetComponent<RectTransform>();
        resetButtonRectTransform.sizeDelta = new Vector2(40, 40);
        resetButtonRectTransform.anchorMin = new Vector2(1, 0);             // Bottom-right of the canvas
        resetButtonRectTransform.anchorMax = new Vector2(1, 0);             // Bottom-right of the canvas
        resetButtonRectTransform.pivot = new Vector2(0.5f, 0.5f);           // Center the pivot
        resetButtonRectTransform.anchoredPosition = new Vector2(-80, 70);   // Positions

        // Refresh button set to -1
        resetButton.onClick.AddListener(new Action(() => OnButtonClicked(-1)));

        EventTrigger resetTrigger = resetButtonObject.AddComponent<EventTrigger>();

        // Set up OnPointerEnter event
        EventTrigger.Entry resetEnterEntry = new();
        resetEnterEntry.eventID = EventTriggerType.PointerEnter;
        resetEnterEntry.callback.AddListener((UnityAction<BaseEventData>)((eventData) => { OnPointerEnter(-1); }));
        resetTrigger.triggers.Add(resetEnterEntry);

        // Set up OnPointerExit event
        EventTrigger.Entry resetExitEntry = new();
        resetExitEntry.eventID = EventTriggerType.PointerExit;
        resetExitEntry.callback.AddListener((UnityAction<BaseEventData>)((eventData) => { OnPointerExit(-1); }));
        resetTrigger.triggers.Add(resetExitEntry);

        buttonImages.Add(resetButtonImage);

        // Add text field to display current choice
        GameObject choiceTextGO = new("ChoiceText");
        choiceTextGO.transform.SetParent(minimapChoiceCanvas.transform);

        choiceTMPro = choiceTextGO.AddComponent<TextMeshProUGUI>();
        choiceTMPro.fontSize = 25;
        choiceTMPro.alignment = TextAlignmentOptions.Center;

        RectTransform choiceTextRectTransform = choiceTextGO.GetComponent<RectTransform>();
        choiceTextRectTransform.sizeDelta = new Vector2(500, 80);
        choiceTextRectTransform.anchorMin = new Vector2(1, 0);              // Bottom-right of the canvas
        choiceTextRectTransform.anchorMax = new Vector2(1, 0);              // Bottom-right of the canvas
        choiceTextRectTransform.pivot = new Vector2(0.5f, 0.5f);            // Center the pivot

        float x = (-canvasSize.x / 2) + canvasOffset.x;                     // Center horizontally based on the minimap size
        float y = canvasSize.y + canvasOffset.y + 25;                       // Over the minimap, with an extra offset to 
                                                                            // prevent superposing the text on the minimap

        choiceTextRectTransform.anchoredPosition = new Vector2(x, y);

        // Add text field to display instructions
        GameObject instructionsTextGO = new("InstructionsText");
        instructionsTextGO.transform.SetParent(minimapChoiceCanvas.transform);

        TextMeshProUGUI instructionsTMPro = instructionsTextGO.AddComponent<TextMeshProUGUI>();
        instructionsTMPro.text = $"<i>Click on the number of your choice. <b>{Plugin.MAP_KEY.Value}</b> to close.</i>";
        instructionsTMPro.fontSize = 16;
        instructionsTMPro.alignment = TextAlignmentOptions.Center;

        RectTransform instructionsTextRectTransform = instructionsTextGO.GetComponent<RectTransform>();
        instructionsTextRectTransform.sizeDelta = new Vector2(500, 50);
        instructionsTextRectTransform.anchorMin = new Vector2(1, 0);              // Bottom-right of the canvas
        instructionsTextRectTransform.anchorMax = new Vector2(1, 0);              // Bottom-right of the canvas
        instructionsTextRectTransform.pivot = new Vector2(0.5f, 0.5f);            // Center the pivot

        float xx = (-canvasSize.x / 2) + canvasOffset.x;                          // Center horizontally based on the minimap size
        float yy = 15;                                                            // Under the minimap

        instructionsTextRectTransform.anchoredPosition = new Vector2(xx, yy);

        DisplaySpawnPoint();
    }

    private void Update()
    {
        // Handle opening / closing the map
        if (Input.GetKeyDown(Plugin.MAP_KEY.Value))
        {
            // Ensure custom game, and do not allow opening the map if the game has started
            if (Utils.IsCustomGame() && !Utils.HasGameStarted())
            {
                // Change opened state
                if (this.GetComponent<Canvas>() != null)
                {
                    bool state = this.GetComponent<Canvas>().enabled;
                    if (state) HideMap(); else ShowMap();
                }
            }
        }
    }

    public void ShowMap()
    {
        if (this.GetComponent<Canvas>() != null)
            this.GetComponent<Canvas>().enabled = true;

        buttonImages.ForEach(img => img.gameObject.SetActive(true));
    }

    public void HideMap()
    {
        if (this.GetComponent<Canvas>() != null)
            this.GetComponent<Canvas>().enabled = false;

        // We must also SetActive(false) to prevent the buttons from being navigable
        // using arrow keys while the map is hidden
        buttonImages.ForEach(img => img.gameObject.SetActive(false));
    }

    public void OnPointerEnter(int ind)
    {          
        if (ind == -1) // Reset button
        {
            buttonImages[^1].color = new Color(Color.red.r, Color.red.g, Color.red.b, 0.85f);
        }
        else
        {
            Color original = buttonImages[ind].color;
            buttonImages[ind].color = new Color(original.r, original.g, original.b, 0.85f);
        }
    }

    public void OnPointerExit(int ind)
    {
        if (ind == -1) // Reset button
        {
#if DEBUG
            buttonImages[^1].color = new Color(1, 0, 0, 0.6f);
#else
            buttonImages[^1].color = Color.white;
#endif
        }
        else
        {
#if DEBUG
            buttonImages[ind].color = new Color(1, 0, 0, 0.6f);
#else
            buttonImages[ind].color = Color.clear; // transparent button
#endif
        }
    }

    public void OnButtonClicked(int spawnIndex)
    {
        // Reset button
        if (spawnIndex == -1)
        {
            SpawnHandler.chosenSpawnPoint = 0;
            SpawnHandler.chosenSpawnPoint_Unmapped = 0;
            DisplaySpawnPoint();
        }
        else
        {
            // Set chosen index to be accessed globally AFTER mapping to real in-game spawn point order
            SpawnHandler.chosenSpawnPoint_Unmapped = spawnIndex;
            SpawnHandler.chosenSpawnPoint = spawnPointOrderMapping[spawnIndex];
            DisplaySpawnPoint();
        }
    }

    private void DisplaySpawnPoint()
    {
        choiceTMPro.text = $"<b>Spawn point: <color=red>{SpawnHandler.shownSpawnPoint}</color></b>";

#if DEBUG
        choiceTMPro.text += $"<color=red> (mapped: {SpawnHandler.chosenSpawnPoint})</color>";
#endif

        MinimapLifecycle.showRecords.RefreshRecordsTables();
    }
}
