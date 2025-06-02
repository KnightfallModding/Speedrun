using TMPro;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UniverseLib;

namespace Speedrun;
public class SpawnMap : MonoBehaviour
{
    private static string DEFAULT_SPAWN_TEXT = "Default spawn point (Study)";

    private Vector2[] spawnPointCoordInJPG =
    {
            new Vector2(40, 1640),   // 1 - added X offset by hand
            new Vector2(218, 1640),
            new Vector2(139, 1446),
            new Vector2(369, 1590),  // 4
            new Vector2(568, 1445),
            new Vector2(817, 1485),
            new Vector2(925, 1615),
            new Vector2(1130, 1460), // 8
            new Vector2(1294, 1595),
            new Vector2(1361, 1471), // 10
            new Vector2(1680, 1332),
            new Vector2(1748, 800),  // 12 - added Y offset by hand
            new Vector2(1485, 350),  // 13 - added Y offset by hand
            new Vector2(1624, 180)   // 14 - added Y offset by hand
    };

    public int[] spawnPointOrderMapping = // I won't even explain how I obtained these... xd
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

    private static Vector2 minimapSize = new(500, 500); // NOTE: Do NOT change this. Minimap coords are supposed to be dynamic but for some reason it does not work at all with other values than (500, 500)
    private static Vector2 spawnButtonSize = new Vector2(30, 30);
    private static TextMeshProUGUI choiceTMPro;
    public static int chosenSpawnPoint = -1;
    private static int chosenSpawnPoint_Unmapped = -1;
    private static List<Image> buttonImages = new List<Image>();

    public void Start()
    {
        var logger = BepInEx.Logging.Logger.CreateLogSource("SpawnMap");

        // 1. Load minimap image & find canvas
        Sprite minimapSprite = Utils.LoadMinimapSprite();

        logger.LogMessage($"Creating minimap with choices...");

        Canvas minimapChoiceCanvas = this.gameObject.GetComponent<Canvas>();

        // 2. Create an Image UI element to display the minimap
        GameObject minimapObject = new GameObject("MinimapChoiceImage");
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

        Vector2 canvasOffset = new Vector2(-20, 30); // 20 pixels from bottom and 30 for right
        Vector2 canvasSize = minimapSize;

        // Calculate the scale multiplier based on the original image dimensions and the canvas size
        float scaleMultiplierX = canvasSize.x / originalWidth;
        float scaleMultiplierY = canvasSize.y / originalHeight;

        // Use the smaller scale to ensure the aspect ratio is preserved
        float scaleFactor = Mathf.Min(scaleMultiplierX, scaleMultiplierY);

        // float scaleMultiplier = ((originalHeight / canvasSize.x) + (originalWidth / canvasSize.y)) / 2 ;

        // Set the position and size of the minimap
        rectTransform.anchoredPosition = canvasOffset;
        rectTransform.sizeDelta = new Vector2(500, 500);

        logger.LogMessage($"Minimap instantiated !");

        // Add a button on each spawn point of the map
        // Need to get the spawnpoint coordinates on the minimap JPEG
        // ..........................................................
        Button[] choiceButtons = new Button[14];
        int i = 0;
        foreach (Vector2 jpgPoint in spawnPointCoordInJPG)
        {
            // Scale the points based on the scale factor
            float scaledX = jpgPoint.x * scaleFactor;
            float scaledY = jpgPoint.y * scaleFactor;
            // logger.LogMessage($"ScaledX = {scaledX} | ScaledY = {scaledY}");

            // Create a button at the calculated position
            GameObject buttonObject = new GameObject($"SpawnPointButton{i}");
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

            Vector2 buttonPosition = new Vector2(finalX, finalY);

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
            EventTrigger.Entry enterEntry = new EventTrigger.Entry();
            enterEntry.eventID = EventTriggerType.PointerEnter;
            enterEntry.callback.AddListener((eventData) => { OnPointerEnter(spawnIndex); });
            trigger.triggers.Add(enterEntry);

            // Set up OnPointerExit event
            EventTrigger.Entry exitEntry = new EventTrigger.Entry();
            exitEntry.eventID = EventTriggerType.PointerExit;
            exitEntry.callback.AddListener((eventData) => { OnPointerExit(spawnIndex); });
            trigger.triggers.Add(exitEntry);

            // Store the button reference
            choiceButtons[i] = spawnButton;
            i++;
        }

        // Add a button to reset the current choice to 'random'
        GameObject resetButtonObject = new GameObject($"ResetChoiceButton");
        Button resetButton = resetButtonObject.AddComponent<Button>();
        Image resetButtonImage = resetButtonObject.AddComponent<Image>();
        resetButtonObject.transform.SetParent(minimapChoiceCanvas.transform);

        AssetBundle assetBundle = Utils.LoadOrGetKnightfallBundle();
        Sprite resetButtonSprite = null;

        Texture2D texture = assetBundle.LoadAsset<Texture2D>("Assets/Sprites/refresh.png");
        if (texture != null)
        {
            // Convert the Texture2D to a Sprite (if necessary for UI)
            resetButtonSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }

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
        EventTrigger.Entry resetEnterEntry = new EventTrigger.Entry();
        resetEnterEntry.eventID = EventTriggerType.PointerEnter;
        resetEnterEntry.callback.AddListener((eventData) => { OnPointerEnter(-1); });
        resetTrigger.triggers.Add(resetEnterEntry);

        // Set up OnPointerExit event
        EventTrigger.Entry resetExitEntry = new EventTrigger.Entry();
        resetExitEntry.eventID = EventTriggerType.PointerExit;
        resetExitEntry.callback.AddListener((eventData) => { OnPointerExit(-1); });
        resetTrigger.triggers.Add(resetExitEntry);

        buttonImages.Add(resetButtonImage);

        // Add text field to display current choice
        GameObject choiceTextGO = new GameObject("ChoiceText");
        choiceTextGO.transform.SetParent(minimapChoiceCanvas.transform);

        choiceTMPro = choiceTextGO.AddComponent<TextMeshProUGUI>();
        choiceTMPro.text = DEFAULT_SPAWN_TEXT;
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
        GameObject instructionsTextGO = new GameObject("InstructionsText");
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
        {
            this.GetComponent<Canvas>().enabled = true;
        }
    }

    public void HideMap()
    {
        if (this.GetComponent<Canvas>() != null)
        {
            this.GetComponent<Canvas>().enabled = false;
        }
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
            chosenSpawnPoint = -1;
            chosenSpawnPoint_Unmapped = -1;
            choiceTMPro.text = DEFAULT_SPAWN_TEXT;
        }
        else
        {
            // Set chosen index to be accessed globally AFTER mapping to real in-game spawn point order
            chosenSpawnPoint_Unmapped = spawnIndex;
            chosenSpawnPoint = spawnPointOrderMapping[spawnIndex];
            choiceTMPro.text = $"<b>Forced Spawn point: <color=red>{chosenSpawnPoint_Unmapped + 1}</color></b>";

            #if DEBUG
            choiceTMPro.text += $"<color=red> (unmapped: {chosenSpawnPoint})</color>";
            #endif
        }
    }
}
