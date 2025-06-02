using Il2Cpp;
using MelonLoader;
using UnityEngine;
using Il2CppTMPro;

namespace Speedrun;

public class Timer : MonoBehaviour
{
    private bool hasStarted = false;
    public bool hasFinished = true;
    private float timer = 0f;
    private TextMeshProUGUI timerText;

    private void Start()
    {
        hasStarted = true;
        timerText = this.GetComponent<TextMeshProUGUI>();
    }

    private void Update() {
        if (hasStarted) {
            timer += Time.deltaTime;
        }

        #if DEBUG
        if (Input.GetKeyDown(KeyCode.F10)) {
            timer += 10f;
        }
        if (Input.GetKeyDown(KeyCode.N)) {
            GameObject horse = Player.localplayer.data.belovedHorse.gameObject;
            horse.transform.position = new Vector3(-11f, 256.9f, 2200f); // Castle
        }

        if (Input.GetKeyDown(KeyCode.B)) {
            GameObject horse = Player.localplayer.data.belovedHorse.gameObject;
            horse.transform.position = transform.position = new Vector3(-523f, 156f, 580f); // Court
        }
        #endif
    }

    private void OnGUI()
    {
        if (timerText != null)
        {
            timerText.text = GetTimerFullText();
        }
    }

    public string GetTimerFullText()
    {
        // This is required to display the 'expected' spawn point to the user.
        // This corresponds to the actual numbers shown on the SpeedrunMap.
        int spawn = SpawnMap.shownSpawnPoint;
        float record = RecordsHandler.GetRecord(spawn-1);

        string formattedBest = record != -1 ? GetFormattedTime(record) : "N/A";
        
        // Change color based on time comparison with records
        bool isRecord = RecordsHandler.IsRecord(spawn - 1, timer);

        string text = "";
        text += "<size=45%>";
        text += $"Spawn {spawn} ";
        text += $"(best: {formattedBest})\n";
        text += "</size>";

        text += isRecord ? "<color=green>" : "<color=red>";
        text += GetFormattedTime(this.timer);
        text += "</color>";

        return text;
    }

    public void StopTimer()
    {
        hasStarted = false;
    }

    public float GetTime()
    {
        return this.timer;
    }

    public string GetFormattedTime()
    {
        return GetFormattedTime(this.timer);
    }

    public static string GetFormattedTime(float _timer)
    {
        // Format timer to 00:00.00
        int minutes = Mathf.FloorToInt(_timer / 60);
        int seconds = Mathf.FloorToInt(_timer % 60);
        int milliseconds = Mathf.FloorToInt((_timer * 100) % 100);

        return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
    }
}