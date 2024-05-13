using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

public class GameSaveData
{
    [Serializable]
    public class LevelScore
    {
        public string Level;
        public int Score;
    }

    [Serializable]
    public class SaveData
    {
        public string CurrentMainLevel;
        public string CurrentSubLevel;
        public GameMode CurrentGameMode;
        public int CurrentScore;
        public List<LevelScore> FinishedLevelScores = new List<LevelScore>();
    }
}
