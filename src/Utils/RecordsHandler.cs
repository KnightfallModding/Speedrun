using Il2Cpp;
using System;
using UnityEngine;
using MelonLoader;
using System.Linq;

namespace Speedrun;

public static class RecordsHandler
{
    private const string RECORDS_PREFS_KEY = "speedrun_records";
    public static readonly float[] DEFAULT_RECORDS = GetDefaultRecordsList();
    private static float[] records = DEFAULT_RECORDS;

    public static float[] GetDefaultRecordsList() => [.. Enumerable.Repeat(-1f, 14)];
    public static float[] GetRecords() => records;

    public static float[] ParseString(string recordsStr) => recordsStr.Split(';').Select(float.Parse).ToArray();
    public static string AsString() => string.Join(";", records);
    public static string AsString(float[] _records) => string.Join(";", _records);

    public static void LoadRecords() => records = ParseString(PlayerPrefs.GetString(RECORDS_PREFS_KEY, AsString(DEFAULT_RECORDS)));
    public static void SaveRecords()
    {
        PlayerPrefs.SetString(RECORDS_PREFS_KEY, RecordsHandler.AsString());
        PlayerPrefs.Save();
    }

    public static bool IsRecord(int spawnPoint, float time)
    {
        if (records == null || spawnPoint < 0 || spawnPoint >= records.Length)
            return false;

        // If there is no previous record it's always a record
        float oldRecord = GetRecord(spawnPoint);
        if (oldRecord == -1)
            return true;

        return time <= oldRecord; // '<=' so that the time is shown in green when the run ends on a record
    }

    public static float GetRecord(int spawnPoint)
    {
        if (records == null || spawnPoint < 0 || spawnPoint >= records.Length)
            return -1;

        return GetRecords()[spawnPoint];
    }

    /// <summary>
    /// Updates a record and automatically saves the new value in PlayerPrefs.
    /// </summary>
    /// <param name="spawnPoint"></param>
    /// <param name="time"></param>
    public static void UpdateRecord(int spawnPoint, float time)
    {
        if (records == null || spawnPoint < 0 || spawnPoint >= records.Length)
            return;

        Melon<Plugin>.Logger.Msg($"Updating record for spawn {spawnPoint + 1} to {time}...");

        records[spawnPoint] = time;
        SaveRecords();
    }
}