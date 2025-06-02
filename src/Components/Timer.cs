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
    private float[] records;

    private void Start()
    {
        hasStarted = true;
        timerText = this.GetComponent<TextMeshProUGUI>();
        records = Utils.GetRecordsList();
        #if DEBUG
        Melon<Plugin>.Logger.Msg($"Loaded records from config: {string.Join("; ", records)}");
        #endif
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
            horse.transform.position = new Vector3(-11f, 256.9f, 2200f); // Castle
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
        int spawn = SpawnMap.shownSpawnPoint; // This is required to display the 'expected' spawn point to the user.
                                              // This corresponds to the actual numbers shown on the SpeedrunMap.
        float record = GetRecord(spawn-1);

        string formattedBest = record != -1 ? GetFormattedTime(record) : "N/A";

        string text = $"<size=45%>Spawn {spawn} (best: {formattedBest})\n</size>";
        // Change color based on time comparison with records
        if (IsRecord(spawn-1, timer))
        {
            text += "<color=green>";
        }
        else
        {
            text += "<color=red>";
        }
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

    public float GetRecord(int spawnPoint)
    {
        if (records == null || spawnPoint < 0 || spawnPoint >= records.Length)
            return -1;
        
        return records[spawnPoint];
    }

    public bool IsRecord(int spawnPoint, float time)
    {
        if (records == null || spawnPoint < 0 || spawnPoint >= records.Length)
            return false;

        // If there is no previous record it's always a record
        if (records[spawnPoint] == -1)
            return true;

        return time <= records[spawnPoint]; // '<=' so that the time is shown in green when the run ends on a record
    }

    public void UpdateRecord(int spawnPoint, float time)
    {
        if (records == null || spawnPoint < 0 || spawnPoint >= records.Length)
            return;

        Melon<Plugin>.Logger.Msg($"Updating record for spawn {spawnPoint+1} to {time}...");

        records[spawnPoint] = time;
        Utils.SaveRecords(records);
    }
}