using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/LevelSettings")]
public class LevelSettings : ScriptableObject
{
    [Serializable]
    public class LevelData
    {
        public string Level;
        public string[] SubLevels;
        public GameMode[] GameModes;
    }

    public List<LevelData> LevelDatas;

    public int GetMainLevelIndex(string level)
    {
        int mainLevelIndex = LevelDatas.FindIndex(x => x.Level == level);
        return mainLevelIndex;
    }

    public int GetSublevelIndex(string level, string sublevel)
    {
        LevelData levelData = LevelDatas.Find(x => x.Level == level);

        if (levelData != null)
        {
            int sublevelIndex = Array.IndexOf(levelData.SubLevels, sublevel);
            return sublevelIndex;
        }

        return -1; // Indicate sublevel not found
    }

    public string GetNextLevel(string level)
    {
        bool isLastLevel = LevelDatas.Last().Level == level;
        string nextLevel = isLastLevel ? LevelDatas[0].Level : LevelDatas[LevelDatas.FindIndex(x => x.Level == level) + 1].Level;

        return nextLevel;
    }

    public string GetPreviousLevel(string level)
    {
        bool isFirstLevel = LevelDatas.First().Level == level;
        string previousLevel = isFirstLevel ? LevelDatas.Last().Level : LevelDatas[LevelDatas.FindIndex(x => x.Level == level) - 1].Level;

        return previousLevel;
    }

    public string GetNextSublevel(string level, string sublevel)
    {
        LevelData levelData = LevelDatas.Find(x => x.Level == level);

        if (levelData != null)
        {
            int sublevelIndex = Array.IndexOf(levelData.SubLevels, sublevel);
            bool isLastSublevel = levelData.SubLevels.Last() == sublevel;
            string nextSublevel = isLastSublevel ? levelData.SubLevels[0] : levelData.SubLevels[sublevelIndex + 1];

            return nextSublevel;
        }

        return null; // Indicate sublevel not found
    }

    public string GetPreviousSubLevel(string level, string sublevel)
    {
        LevelData levelData = LevelDatas.Find(x => x.Level == level);

        if (levelData != null)
        {
            int sublevelIndex = Array.IndexOf(levelData.SubLevels, sublevel);
            bool isFirstSublevel = levelData.SubLevels.First() == sublevel;
            string previousSublevel = isFirstSublevel ? null : levelData.SubLevels[sublevelIndex - 1];

            return previousSublevel;
        }

        return null; // Indicate sublevel not found
    }

    public string GetLastSublevel(string level)
    {
        LevelData levelData = LevelDatas.Find(x => x.Level == level);

        if (levelData != null)
        {
            return levelData.SubLevels.Last();
        }

        return null; // Indicate sublevel not found
    }

    public GameMode GetGameMode(string level, string sublevel)
    {
        LevelData levelData = LevelDatas.Find(x => x.Level == level);

        if (levelData != null)
        {
            int sublevelIndex = Array.IndexOf(levelData.SubLevels, sublevel);
            return levelData.GameModes[sublevelIndex];
        }

        return GameMode.Solo; // Indicate sublevel not found
    }
}

public enum GameMode
{
    Solo,
    PVE
}