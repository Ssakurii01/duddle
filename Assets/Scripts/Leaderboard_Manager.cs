using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores the top-N scores. Persisted via PlayerPrefs as a single string
/// of "name|score;name|score;..." entries — no JSON dependency, no extra files.
/// Mirrors the Data_Manager / File_Manager split: this class holds the data
/// in memory; File_Manager tells it when to load and save.
/// </summary>
public static class Leaderboard_Manager
{
    public const int MaxEntries = 10;
    private const string PrefsKey = "Leaderboard";

    [Serializable]
    public struct Entry
    {
        public string Name;
        public int Score;

        public Entry(string name, int score) { Name = name; Score = score; }
    }

    private static List<Entry> entries = new List<Entry>();

    public static IReadOnlyList<Entry> Get_Entries() => entries;

    // The most recent run — tracked separately so the leaderboard panel can
    // still surface the latest game's score even when the player's all-time
    // best is higher.
    public static string LastSessionName  { get; private set; } = "";
    public static int    LastSessionScore { get; private set; } = -1;
    public static bool   HasLastSession   => LastSessionScore >= 0;

    /// <summary>
    /// Record a session's final score for a player. Each player keeps
    /// exactly one entry — their BEST score — so replaying the game
    /// updates an existing row instead of stacking duplicates.
    /// The list is kept sorted by score (descending) and trimmed to MaxEntries.
    /// </summary>
    public static void Add_Score(string name, int score)
    {
        if (string.IsNullOrWhiteSpace(name)) name = "Player";
        // Strip separators so a wild name can't corrupt the prefs string.
        name = name.Replace("|", "").Replace(";", "").Trim();
        if (name.Length > 16) name = name.Substring(0, 16);

        // Remember the run we just finished so the UI can show it as
        // "Your last game" even if the player's all-time best is higher.
        LastSessionName  = name;
        LastSessionScore = score;

        // If this player is already on the board, just keep their highest.
        int existing = entries.FindIndex(e => e.Name == name);
        if (existing >= 0)
        {
            if (score > entries[existing].Score)
                entries[existing] = new Entry(name, score);
        }
        else
        {
            entries.Add(new Entry(name, score));
        }

        // Always keep the board sorted by score, biggest first.
        entries.Sort((a, b) => b.Score.CompareTo(a.Score));

        if (entries.Count > MaxEntries)
            entries.RemoveRange(MaxEntries, entries.Count - MaxEntries);
    }

    public static void Clear()
    {
        entries.Clear();
    }

    // ---------- persistence ----------

    public static void Load()
    {
        entries.Clear();
        string raw = PlayerPrefs.GetString(PrefsKey, "");
        if (string.IsNullOrEmpty(raw)) return;

        string[] rows = raw.Split(';');
        foreach (string row in rows)
        {
            if (string.IsNullOrEmpty(row)) continue;
            string[] parts = row.Split('|');
            if (parts.Length != 2) continue;

            int score;
            if (!int.TryParse(parts[1], out score)) continue;
            entries.Add(new Entry(parts[0], score));
        }
        // Preserve stored order — most-recent-first, not sorted by score.
    }

    public static void Save()
    {
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < entries.Count; i++)
        {
            if (i > 0) sb.Append(';');
            sb.Append(entries[i].Name).Append('|').Append(entries[i].Score);
        }
        PlayerPrefs.SetString(PrefsKey, sb.ToString());
        PlayerPrefs.Save();
    }
}
