using System;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace ESS.UI.GamePlay
{
	public static class GameHistoryStore
	{
		private const string FileName = "game_history.json";

		[Serializable]
		private class HistoryFile
		{
			public GameHistoryEntry[] entries;
		}

		[Serializable]
		public class GameHistoryEntry
		{
			public string characterName;
			public int score;
			public int level;
			public string playedAtUtc;
		}

		public static void RecordRun(string characterName, int score, int level)
		{
			HistoryFile history = LoadHistory();
			GameHistoryEntry[] entries = history.entries ?? new GameHistoryEntry[0];
			Array.Resize(ref entries, entries.Length + 1);
			entries[entries.Length - 1] = new GameHistoryEntry
			{
				characterName = string.IsNullOrEmpty(characterName) ? "Unknown" : characterName,
				score = score,
				level = level,
				playedAtUtc = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)
			};

			history.entries = entries;
			try
			{
				File.WriteAllText(GetFilePath(), JsonUtility.ToJson(history, true));
			}
			catch (Exception exception)
			{
				Debug.LogError($"Could not save game history: {exception.Message}");
			}
		}

		public static GameHistoryEntry[] GetLeaderboard(int maxEntries)
		{
			GameHistoryEntry[] entries = LoadHistory().entries ?? new GameHistoryEntry[0];
			Array.Sort(entries, (left, right) => right.score.CompareTo(left.score));

			if (maxEntries < entries.Length)
			{
				Array.Resize(ref entries, Math.Max(0, maxEntries));
			}

			return entries;
		}

		private static HistoryFile LoadHistory()
		{
			string path = GetFilePath();
			if (!File.Exists(path))
			{
				return new HistoryFile { entries = new GameHistoryEntry[0] };
			}

			try
			{
				HistoryFile history = JsonUtility.FromJson<HistoryFile>(File.ReadAllText(path));
				return history ?? new HistoryFile { entries = new GameHistoryEntry[0] };
			}
			catch (Exception exception)
			{
				Debug.LogWarning($"Could not read game history: {exception.Message}");
				return new HistoryFile { entries = new GameHistoryEntry[0] };
			}
		}

		private static string GetFilePath()
		{
			return Path.Combine(Application.persistentDataPath, FileName);
		}
	}
}