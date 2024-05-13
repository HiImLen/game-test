using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EasyButtons;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Transistion")]
    [SerializeField] private GameObject _transistionPanel;

    [Header("UI")]
    [SerializeField] private Image _soundImage;
    [SerializeField] private Sprite _muteSound;
    [SerializeField] private Sprite _unmuteSound;

    [Header("Score")]
    [SerializeField] private TMPro.TextMeshProUGUI _scoreText;
    [SerializeField] private GameObject _comboHit;

    [Header("Win")]
    [SerializeField] private GameObject _winPanel;
    [SerializeField] private TMPro.TextMeshProUGUI _winScoreText;
    [SerializeField] private TMPro.TextMeshProUGUI _winBestScoreText;

    [Header("Lose")]
    [SerializeField] private GameObject _losePanel;
    [SerializeField] private TMPro.TextMeshProUGUI _loseScoreText;
    [SerializeField] private TMPro.TextMeshProUGUI _loseBestScoreText;

    [Header("Progression")]
    [SerializeField] private Color _mainLevelColor;
    [SerializeField] private Color _subLevelColor;
    [SerializeField] private Color _lockedColor;
    [SerializeField] private GameObject _progressionPanel;
    [SerializeField] private TMPro.TextMeshProUGUI _currentLevelText;
    [SerializeField] private TMPro.TextMeshProUGUI _nextLevelText;
    [SerializeField] private List<Image> _subLevelBackgrounds;
    [SerializeField] private List<Image> _subLevelForegrounds;

    [Button]
    public void ShowTransistionPanel()
    {
        GameManager.Instance.ActivateGO(_transistionPanel);
    }

    [Button]
    public void HideTransistionPanel()
    {
        GameManager.Instance.DisableGO(_transistionPanel);
        GameManager.Instance.UpdateGameState(GameState.Start);
    }

    [Button]
    public void ShowWinPanel(int score, string currentMainLevel)
    {
        _winScoreText.text = score.ToString();

        int bestScore = GameManager.Instance.DataManager.SaveData.FinishedLevelScores
            .Where(x => x.Level == currentMainLevel)
            .Select(x => x.Score)
            .FirstOrDefault();
        _winBestScoreText.text = bestScore.ToString();

        GameManager.Instance.ActivateGO(_winPanel);
    }

    [Button]
    public void ShowLosePanel(int score, string currentMainLevel)
    {
        _loseScoreText.text = score.ToString();

        int bestScore = GameManager.Instance.DataManager.SaveData.FinishedLevelScores
            .Where(x => x.Level == currentMainLevel)
            .Select(x => x.Score)
            .FirstOrDefault();
        _loseBestScoreText.text = bestScore.ToString();

        GameManager.Instance.ActivateGO(_losePanel);
    }

    public void ShowComboHit()
    {
        GameManager.Instance.ActivateGO(_comboHit);
    }

    public void ShowProgress()
    {
        GameManager.Instance.ActivateGO(_progressionPanel);
        _currentLevelText.text = GameManager.Instance.DataManager.SaveData.CurrentMainLevel;
        string nextLevel = GameManager.Instance.DataManager.LevelSettings.GetNextLevel(GameManager.Instance.DataManager.SaveData.CurrentMainLevel);
        _nextLevelText.text = nextLevel;
        int currentSubLevelIndex = GameManager.Instance.DataManager.LevelSettings.GetSublevelIndex(GameManager.Instance.DataManager.SaveData.CurrentMainLevel, GameManager.Instance.DataManager.SaveData.CurrentSubLevel);
        for (int i = 0; i < _subLevelBackgrounds.Count; i++)
        {
            if (i < currentSubLevelIndex)
            {
                _subLevelBackgrounds[i].color = Color.white;
                _subLevelForegrounds[i].color = _mainLevelColor;
            }
            else if (i == currentSubLevelIndex)
            {
                _subLevelBackgrounds[i].color = _subLevelColor;
                _subLevelForegrounds[i].color = _lockedColor;
            }
            else
            {
                _subLevelBackgrounds[i].color = Color.white;
                _subLevelForegrounds[i].color = _lockedColor;
            }
        }
    }

    public void UpdateScore(int score)
    {
        _scoreText.text = score.ToString();
    }

    public void OnSoundClick()
    {
        GameManager.Instance.AudioManager.ToggleMute();
        if (GameManager.Instance.AudioManager.IsMuted == true)
        {
            _soundImage.sprite = _muteSound;
        }
        else
        {
            _soundImage.sprite = _unmuteSound;
        }
    }
}
