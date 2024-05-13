using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IngameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private GameMode _gameMode;
    [SerializeField] private List<GameObject> _subLevels;
    [SerializeField] private GameObject _touchInput;
    [SerializeField] private GameObject _startInput;

    [Header("Color")]
    public Color Uncolored;
    public Color Colored;

    [Header("Game Info")]
    public List<GameObject> Rubbers;
    public bool IsGameStarted = false;
    public int CurrentScore { get; private set; }
    private BrushController _brushController;
    private bool _doneLoadLevel = false;
    private string _currentSubLevel;

    [Header("PvE Settings")]
    private AIBrushController _AIBrushController;
    [SerializeField] private float _timeCountDown = 30f;

    void Awake()
    {
        GameManager.OnGameStateChangeEvent += OnGameStateChange;
        _brushController = FindObjectOfType<BrushController>();
        _brushController.SetParticleColor(Colored);
    }

    void OnDestroy()
    {
        GameManager.OnGameStateChangeEvent -= OnGameStateChange;
    }

    void Update()
    {
        if (IsGameStarted)
        {
            if (_gameMode == GameMode.Solo)
            {
                if (Rubbers.Count == 0)
                {
                    GameManager.Instance.UpdateGameState(GameState.Win);
                }
            }
            else if (_gameMode == GameMode.PVE)
            {
                _timeCountDown -= Time.deltaTime;
                GameManager.Instance.UIManager.UpdateScore((int)_timeCountDown);
                if (_timeCountDown <= 0)
                {
                    PvEEvaluate();
                }
            }
        }
    }

    private void OnGameStateChange(GameState state)
    {
        switch (state)
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
            default:
                break;
        }
    }

    private void ColorTile()
    {

        GameObject[] rubberObjects = GameObject.FindGameObjectsWithTag("Tile");
        foreach (GameObject rubber in rubberObjects)
        {
            Rubbers.Add(rubber);
        }

        // The tile is static batching so we need to set the color of the tile by using MaterialPropertyBlock
        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        propertyBlock.SetColor("_Color", Colored);

        foreach (GameObject rubber in Rubbers)
        {
            rubber.GetComponent<Renderer>().SetPropertyBlock(propertyBlock);
        }

        if (_gameMode == GameMode.Solo)
        {
            propertyBlock.SetColor("_Color", Uncolored);
            foreach (GameObject rubber in Rubbers)
            {
                foreach (Transform child in rubber.transform)
                {
                    child.GetComponent<Renderer>().SetPropertyBlock(propertyBlock);
                }
            }
        }
        else if (_gameMode == GameMode.PVE)
        {
            // color first half as colored, second half as uncolored
            int halfCount = Rubbers.Count / 2;
            propertyBlock.SetColor("_Color", Uncolored);
            for (int i = 0; i < halfCount; i++)
            {
                foreach (Transform child in Rubbers[i].transform)
                {
                    child.GetComponent<Renderer>().SetPropertyBlock(propertyBlock);
                }
            }

            propertyBlock.SetColor("_Color", Colored);
            for (int i = halfCount; i < Rubbers.Count; i++)
            {
                foreach (Transform child in Rubbers[i].transform)
                {
                    child.GetComponent<Renderer>().SetPropertyBlock(propertyBlock);
                }
            }
        }

    }

    private void LoadCurrentSubLevel()
    {
        _currentSubLevel = GameManager.Instance.DataManager.SaveData.CurrentSubLevel;
        _gameMode = GameManager.Instance.DataManager.SaveData.CurrentGameMode;

        foreach (GameObject subLevel in _subLevels)
        {
            if (subLevel.name == _currentSubLevel)
            {
                subLevel.SetActive(true);
            }
            else
            {
                subLevel.SetActive(false);
            }
        }
        _doneLoadLevel = true;
    }

    public void AddHitScore(int score)
    {
        _brushController.SetCurrentHit(_brushController.CurrentHit + 1);
        CurrentScore += score;
        GameManager.Instance.UIManager.UpdateScore(CurrentScore);
    }

    public void AddBonusScore(int score)
    {
        CurrentScore += score;
        GameManager.Instance.UIManager.UpdateScore(CurrentScore);
    }

    private void OnReady()
    {
        CurrentScore = GameManager.Instance.DataManager.SaveData.CurrentScore;
        _touchInput.SetActive(false);
        _brushController.SetIsFirstHit(true);
        _brushController.SetIsStartCalculateCombo(false);
        _brushController.SetCurrentHit(0);
        _brushController.SetPreviousHit(0);
        GameManager.Instance.UpdateGameState(GameState.Start);
    }

    private void OnStart()
    {
        StartCoroutine(StartHandler());
    }

    IEnumerator StartHandler()
    {
        LoadCurrentSubLevel();
        yield return new WaitUntil(() => _doneLoadLevel == true);
        ColorTile();
        if (_gameMode == GameMode.PVE)
        {
            _AIBrushController = FindObjectOfType<AIBrushController>();
            _AIBrushController.SetIngameManager(this);
            _AIBrushController.SetParticleColor(Uncolored);
            _brushController.SetParticleColor(Colored);
        }
        yield return new WaitForSeconds(0.5f);
        _AIBrushController?.ShowBrush();
        _brushController.ShowBrush();
        yield return new WaitForSeconds(1.5f);
        GameManager.Instance.ActivateGO(_startInput);
        _touchInput.SetActive(true);
    }

    private void OnIngame()
    {
        IsGameStarted = true;
        _brushController.OnTouchInput();
    }

    private void OnWin()
    {
        GameManager.Instance.AudioManager.PlayWinEffect();
        IsGameStarted = false;
        _brushController.HideBrush();
        if (_gameMode == GameMode.PVE)
        {
            _AIBrushController.HideBrush();
            Rubbers.Clear();
        }
        string currentMainLevel = GameManager.Instance.DataManager.SaveData.CurrentMainLevel;

        if (_currentSubLevel == _subLevels.Last().name)
        {
            GameManager.Instance.DataManager.UpdateLevelScore(CurrentScore);
            GameManager.Instance.UIManager.ShowWinPanel(CurrentScore, currentMainLevel);
        }
        else
        {
            GameManager.Instance.DataManager.UpdateLevelScore(CurrentScore);
            GameManager.Instance.UIManager.ShowTransistionPanel();
        }
    }

    private void OnLose()
    {
        GameManager.Instance.AudioManager.PlayLoseEffect();
        string currentMainLevel = GameManager.Instance.DataManager.SaveData.CurrentMainLevel;
        IsGameStarted = false;
        _brushController.HideBrush();
        if (_gameMode == GameMode.PVE)
        {
            _AIBrushController.HideBrush();
            Rubbers.Clear();
        }
        GameManager.Instance.DataManager.LoseUpdateLevelScore(CurrentScore);
        GameManager.Instance.UIManager.ShowLosePanel(CurrentScore, currentMainLevel);
    }

    private void PvEEvaluate()
    {
        // If number of colored rubber of child in the rubbers list is more than number of uncolored rubber, win
        List<GameObject> childRubbers = new List<GameObject>();
        foreach (GameObject rubber in Rubbers)
        {
            foreach (Transform child in rubber.transform)
            {
                childRubbers.Add(child.gameObject);
            }
        }

        MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
        int coloredCount = 0;
        int uncoloredCount = 0;

        foreach (var childRubber in childRubbers)
        {
            Renderer renderer = childRubber.GetComponent<Renderer>();
            renderer.GetPropertyBlock(propBlock);
            Color color = propBlock.GetColor("_Color");

            if (color == Colored)
            {
                coloredCount++;
            }
            else if (color == Uncolored)
            {
                uncoloredCount++;
            }
        }

        CurrentScore = coloredCount * 2;

        if (coloredCount > uncoloredCount)
        {
            GameManager.Instance.UpdateGameState(GameState.Win);
        }
        else
        {
            GameManager.Instance.UpdateGameState(GameState.Lose);
        }
    }

    public void Respawn()
    {
        _brushController.Respawn();
    }

    public void AIRespawn()
    {
        _AIBrushController.Respawn();
    }
}