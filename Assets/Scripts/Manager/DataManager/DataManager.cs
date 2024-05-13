using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using System.IO;
using System.Linq;

public class DataManager : MonoBehaviour
{
    public LevelSettings LevelSettings;
    public GameSaveData.SaveData SaveData;
    public string SavePath;
    public string SaveName;
    public string SaveExtension;
    public string SaveFile;

    void Awake()
    {
        SavePath = Application.persistentDataPath;
        SaveName = "brushhit";
        SaveExtension = ".fun";
        SaveFile = SavePath + "/" + SaveName + SaveExtension;
        SaveData = LoadGameBinary();
    }

    public void LoseUpdateLevelScore(int score)
    {
        if (SaveData.FinishedLevelScores.Any(x => x.Level == SaveData.CurrentMainLevel))
        {
            GameSaveData.LevelScore levelScore = SaveData.FinishedLevelScores.Find(x => x.Level == SaveData.CurrentMainLevel);
            if (levelScore.Score < score)
            {
                levelScore.Score = score;
            }
        }
        else
        {
            SaveData.FinishedLevelScores.Add(new GameSaveData.LevelScore { Level = SaveData.CurrentMainLevel, Score = score });
        }
        SaveData.CurrentScore = 0;

        SaveGameBinary(SaveData);
    }

    public void UpdateLevelScore(int score)
    {
        // Check if the current sublevel is the last sublevel in the current level
        if (LevelSettings.LevelDatas.Find(x => x.Level == SaveData.CurrentMainLevel).SubLevels.Last() == SaveData.CurrentSubLevel)
        {
            // If it is, check if the current level is already in the finishedLevelScores
            if (SaveData.FinishedLevelScores.Any(x => x.Level == SaveData.CurrentMainLevel))
            {
                // If it is, check if the current score is higher than the previous score
                GameSaveData.LevelScore levelScore = SaveData.FinishedLevelScores.Find(x => x.Level == SaveData.CurrentMainLevel);
                if (levelScore.Score < score)
                {
                    levelScore.Score = score;
                }
            }
            else
            {
                // If it is not, add the level and score to the finishedLevelScores
                SaveData.FinishedLevelScores.Add(new GameSaveData.LevelScore { Level = SaveData.CurrentMainLevel, Score = score });
            }

            // Set the current level to the next level in the LevelSettings and reset score to 0. If the current level is the last level, set the current level to the first level
            bool isLastLevel = LevelSettings.LevelDatas.Last().Level == SaveData.CurrentMainLevel;
            SaveData.CurrentMainLevel = isLastLevel ? LevelSettings.LevelDatas[0].Level : LevelSettings.LevelDatas[LevelSettings.LevelDatas.FindIndex(x => x.Level == SaveData.CurrentMainLevel) + 1].Level;
            int mainLevelIndex = LevelSettings.GetMainLevelIndex(SaveData.CurrentMainLevel);
            SaveData.CurrentSubLevel = isLastLevel ? LevelSettings.LevelDatas[0].SubLevels[0] : LevelSettings.LevelDatas[mainLevelIndex].SubLevels[0];
            SaveData.CurrentGameMode = isLastLevel ? LevelSettings.LevelDatas[0].GameModes[0] : LevelSettings.LevelDatas[mainLevelIndex].GameModes[0];
            SaveData.CurrentScore = 0;
        }
        else
        {
            // If it is not, set the current sublevel to the next sublevel in the LevelSettings
            int subLevelIndex = LevelSettings.GetSublevelIndex(SaveData.CurrentMainLevel, SaveData.CurrentSubLevel);
            SaveData.CurrentSubLevel = LevelSettings.LevelDatas.Find(x => x.Level == SaveData.CurrentMainLevel).SubLevels[subLevelIndex + 1];
            SaveData.CurrentGameMode = LevelSettings.LevelDatas.Find(x => x.Level == SaveData.CurrentMainLevel).GameModes[subLevelIndex + 1];
            SaveData.CurrentScore = score;
        }

        SaveGameBinary(SaveData);
    }

    public void SaveGameBinary(GameSaveData.SaveData data)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(SaveFile);
        bf.Serialize(file, data);
        file.Close();
    }

    public GameSaveData.SaveData LoadGameBinary()
    {
        if (File.Exists(SaveFile))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(SaveFile, FileMode.Open);
            GameSaveData.SaveData data = (GameSaveData.SaveData)bf.Deserialize(file);
            file.Close();
            return data;
        }
        else
        {
            Debug.Log("Save file not found in " + SaveFile);

            GameSaveData.SaveData data = new GameSaveData.SaveData
            {
                CurrentMainLevel = LevelSettings.LevelDatas[0].Level,
                CurrentSubLevel = LevelSettings.LevelDatas[0].SubLevels[0],
                CurrentGameMode = LevelSettings.LevelDatas[0].GameModes[0],
                CurrentScore = 0
            };
            SaveGameBinary(data);

            return data;
        }
    }
}
