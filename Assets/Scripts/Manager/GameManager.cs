using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using System;
using LeanTweenAnimation.UI;
using UnityEngine.SceneManagement;
using EasyButtons;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public AudioManager AudioManager { get; private set; }
    public DataManager DataManager { get; private set; }
    public SceneLoader SceneLoader { get; private set; }
    public UIManager UIManager { get; private set; }

    public GameState State { get; private set; }
    public static event Action<GameState> OnGameStateChangeEvent;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);

        AudioManager = GetComponentInChildren<AudioManager>();
        DataManager = GetComponentInChildren<DataManager>();
        SceneLoader = GetComponentInChildren<SceneLoader>();
        UIManager = GetComponentInChildren<UIManager>();

        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    void Start()
    {
        UpdateGameState(GameState.Ready);
    }

    public void UpdateGameState(GameState newState)
    {
        State = newState;

        switch (State)
        {
            case GameState.Ready:
                OnReady();
                break;
            case GameState.Start:
                OnStart();
                break;
            case GameState.Ingame:
                OnIngame();
                break;
            case GameState.Win:
                OnWin();
                break;
            case GameState.Lose:
                OnLose();
                break;
        }

        OnGameStateChangeEvent?.Invoke(newState);
    }

    public void OnReady()
    {
        // Wait for DataManager to load data at start
        while (DataManager.SaveData == null) { }

        // If current level is not current main level, load current main level
        if (DataManager.SaveData.CurrentMainLevel != SceneManager.GetActiveScene().name)
            SceneLoader.LoadLevel(DataManager.SaveData.CurrentMainLevel);

        UIManager.UpdateScore(DataManager.SaveData.CurrentScore);
        UIManager.ShowProgress();
    }

    public void OnStart()
    {

    }

    public void OnIngame()
    {

    }

    public void OnWin()
    {

    }

    public void OnLose()
    {

    }

    public void ActivateGO(GameObject obj)
    {
        ActivateRecursively(obj);
        obj.SetActive(true);
    }

    private void ActivateRecursively(GameObject obj)
    {
        if (obj.GetComponent<UITween>() != null)
            obj.gameObject.SetActive(true);
        foreach (Transform child in obj.transform)
            ActivateRecursively(child.gameObject);
    }

    public void DisableGO(GameObject obj)
    {
        if (obj.GetComponent<UITween>() != null)
        {
            UITween[] tweens = obj.GetComponents<UITween>();
            foreach (UITween tween in tweens)
                tween.Disable();
        }
        foreach (Transform child in obj.transform)
            DisableGO(child.gameObject);
        obj.SetActive(false);
    }

    [Button]
    private void SetOverrideLevel(string level, string subLevel)
    {
        DataManager.SaveData.CurrentMainLevel = level;
        DataManager.SaveData.CurrentSubLevel = subLevel;
        DataManager.SaveData.CurrentGameMode = DataManager.LevelSettings.GetGameMode(level, subLevel);
        DataManager.SaveData.CurrentScore = 0;
        SceneLoader.LoadLevel(level);
    }

    [Button]
    public void LoadSaveDataCurrentLevel()
    {
        SceneLoader.LoadLevel(DataManager.SaveData.CurrentMainLevel);
    }

    public void SetReadyState()
    {
        StartCoroutine(SetReadyStateDelay());
    }

    public void LoadNextSubLevel()
    {
        string currentMainLevel = DataManager.SaveData.CurrentMainLevel;
        string currentSubLevel = DataManager.SaveData.CurrentSubLevel;

        string nextSubLevel = DataManager.LevelSettings.GetNextSublevel(currentMainLevel, currentSubLevel);

        if (nextSubLevel != null)
        {
            DataManager.SaveData.CurrentSubLevel = nextSubLevel;
        }
        else
        {
            string nextMainLevel = DataManager.LevelSettings.GetNextLevel(currentMainLevel);
            DataManager.SaveData.CurrentMainLevel = nextMainLevel;
            DataManager.SaveData.CurrentSubLevel = "0";
        }

        SetOverrideLevel(DataManager.SaveData.CurrentMainLevel, DataManager.SaveData.CurrentSubLevel);
    }

    public void LoadPreviousSubLevel()
    {
        string currentMainLevel = DataManager.SaveData.CurrentMainLevel;
        string currentSubLevel = DataManager.SaveData.CurrentSubLevel;

        string previousSubLevel = DataManager.LevelSettings.GetPreviousSubLevel(currentMainLevel, currentSubLevel);

        if (previousSubLevel != null)
        {
            DataManager.SaveData.CurrentSubLevel = previousSubLevel;
        }
        else
        {
            string previousMainLevel = DataManager.LevelSettings.GetPreviousLevel(currentMainLevel);
            DataManager.SaveData.CurrentMainLevel = previousMainLevel;
            DataManager.SaveData.CurrentSubLevel = DataManager.LevelSettings.GetLastSublevel(previousMainLevel);
        }

        SetOverrideLevel(DataManager.SaveData.CurrentMainLevel, DataManager.SaveData.CurrentSubLevel);
    }

    private IEnumerator SetReadyStateDelay()
    {
        yield return new WaitForSeconds(0.5f);
        UpdateGameState(GameState.Ready);
    }
}

public enum GameState
{
    Ready,
    Start,
    Ingame,
    Win,
    Lose
}
