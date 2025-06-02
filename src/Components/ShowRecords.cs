using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Speedrun;
public class ShowRecords : MonoBehaviour
{
    private TextMeshProUGUI recordsText1;
    private TextMeshProUGUI recordsText2;

    private void Start()
    {
        Canvas minimapChoiceCanvas = Plugin.spawnMap.GetComponent<Canvas>();
               
        // Add 'Best times' text
        GameObject bestTimesGO = new GameObject("BestTime");
        bestTimesGO.transform.SetParent(minimapChoiceCanvas.transform);

        TextMeshProUGUI bestTimesText = bestTimesGO.AddComponent<TextMeshProUGUI>();
        RectTransform bestTimesRect = bestTimesGO.GetComponent<RectTransform>();

        bestTimesRect.anchorMin = new Vector2(1, 1); // Top-Right anchor
        bestTimesRect.anchorMax = new Vector2(1, 1); // Top-Right anchor
        bestTimesRect.pivot = new Vector2(1, 1);     // Pivot also in top-right corner
        bestTimesRect.anchoredPosition = new Vector2(-150, -120);
        bestTimesText.text = "<u>_Best Times_</u>";
        bestTimesText.fontSize = 25;

        GameObject recordsTablesGO = new GameObject("RecordsTables");
        HorizontalLayoutGroup horizontalLayout = recordsTablesGO.AddComponent<HorizontalLayoutGroup>();
        recordsTablesGO.transform.SetParent(minimapChoiceCanvas.transform);

        horizontalLayout.childControlWidth = false;

        // 3. Set the RectTransform to anchor the records table to the top-right corner
        RectTransform recordsTableRect = recordsTablesGO.GetComponent<RectTransform>();
        recordsTableRect.anchorMin = new Vector2(1, 1); // Top-Right anchor
        recordsTableRect.anchorMax = new Vector2(1, 1); // Top-Right anchor
        recordsTableRect.pivot = new Vector2(1, 1);     // Pivot also in top-right corner
        recordsTableRect.anchoredPosition = new Vector2(-405, -240);

        // Add text field to display records
        // 7 records per table
        GameObject recordsTable1GO = new GameObject("RecordsTable1");
        GameObject recordsTable2GO = new GameObject("RecordsTable2");

        recordsTable1GO.transform.SetParent(recordsTablesGO.transform);
        recordsTable2GO.transform.SetParent(recordsTablesGO.transform);
        
        recordsTable1GO.AddComponent<LayoutElement>();
        recordsTable2GO.AddComponent<LayoutElement>();
        
        recordsText1 = recordsTable1GO.AddComponent<TextMeshProUGUI>();
        recordsText2 = recordsTable2GO.AddComponent<TextMeshProUGUI>();
        
        recordsText1.alignment = TextAlignmentOptions.Left;
        recordsText2.alignment = TextAlignmentOptions.Left;
        recordsText1.fontSize = 23;
        recordsText2.fontSize = 23;
        
        
        RectTransform rectTransform1 = recordsTable1GO.GetComponent<RectTransform>();
        RectTransform rectTransform2 = recordsTable2GO.GetComponent<RectTransform>();
        rectTransform1.sizeDelta = new Vector2(250, 100);
        rectTransform2.sizeDelta = new Vector2(250, 100);

        rectTransform1.anchorMin = new Vector2(1, 1); // Top-Right anchor
        rectTransform1.anchorMax = new Vector2(1, 1); // Top-Right anchor
        rectTransform1.pivot = new Vector2(1, 1);     // Pivot also in top-right corner

        UpdateRecordsTables();
    }

    public void UpdateRecordsTables()
    {
        float[] records = Utils.GetRecordsList();

        string recordsTable1 = "";
        string recordsTable2 = "";

        int m = records.Length / 2;
        for(int i = 0; i < records.Length; i++)
        {
            int spawn = i + 1;
            string formattedRecord = records[i] != -1 ? Timer.GetFormattedTime(records[i]) : "N/A";
            
            // Add leading spaces for 8 and 9 to be aligned with the other double-digits spawn points
            string entry = (spawn == 8 || spawn == 9) ? $"Spawn   {spawn}: {formattedRecord}\n" : $"Spawn {spawn}: {formattedRecord}\n";
            if (spawn == SpawnMap.shownSpawnPoint)
            {
                entry = "<color=yellow>" + entry + "</color>";
            }

            if (i < m) recordsTable1 += entry;
            else recordsTable2 += entry;
        }

        recordsText1.text = recordsTable1;
        recordsText2.text = recordsTable2;
    }
}
