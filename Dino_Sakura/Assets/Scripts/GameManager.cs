using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Speed")]
    [SerializeField] private float startSpeed = 5f;
    [SerializeField] private float maxSpeed = 14f;
    [SerializeField] private float speedIncreasePerSecond = 0.04f;

    [Header("Score")]
    [SerializeField] private float scoreMultiplier = 10f;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI bestScoreText;

    [Header("Panels")]
    [SerializeField] private GameObject idlePanel;
    [SerializeField] private GameObject deadPanel;
    [SerializeField] private GameObject pausePanel;

    [Header("Buttons")]
    [SerializeField] private GameObject pauseButton;

    public GameState CurrentState { get; private set; } = GameState.Idle;
    public float WorldSpeed { get; private set; }
    public float Score { get; private set; }

    private const string BestScoreKey = "DinoSakuraPro_BestScore";
    private float bestScore;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Application.targetFrameRate = 60;
        bestScore = PlayerPrefs.GetFloat(BestScoreKey, 0f);
    }

    private void Start()
    {
        EnterIdle();
    }

    private void Update()
    {
        if (CurrentState == GameState.Idle && Input.GetMouseButtonDown(0))
        {
            StartGame();
        }

        if (CurrentState == GameState.Playing && Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame();
        }
        else if (CurrentState == GameState.Paused && Input.GetKeyDown(KeyCode.Escape))
        {
            ResumeGame();
        }

        if (CurrentState != GameState.Playing)
        {
            UpdateUI();
            return;
        }

        WorldSpeed = Mathf.Min(
            maxSpeed,
            WorldSpeed + speedIncreasePerSecond * Time.deltaTime
        );

        Score += WorldSpeed * scoreMultiplier * Time.deltaTime;

        UpdateUI();
    }

    public void EnterIdle()
    {
        CurrentState = GameState.Idle;
        WorldSpeed = 0f;
        Score = 0f;
        Time.timeScale = 1f;

        SetActiveSafe(idlePanel, true);
        SetActiveSafe(deadPanel, false);
        SetActiveSafe(pausePanel, false);
        SetActiveSafe(pauseButton, false);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopAllAudio();
        }

        UpdateUI();
    }

    public void StartGame()
    {
        if (CurrentState != GameState.Idle) return;

        CurrentState = GameState.Playing;
        WorldSpeed = startSpeed;
        Score = 0f;
        Time.timeScale = 1f;

        SetActiveSafe(idlePanel, false);
        SetActiveSafe(deadPanel, false);
        SetActiveSafe(pausePanel, false);
        SetActiveSafe(pauseButton, true);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
            AudioManager.Instance.PlayGameplayAudio();
        }

        UpdateUI();
    }

    public void PauseGame()
    {
        if (CurrentState != GameState.Playing) return;

        CurrentState = GameState.Paused;
        Time.timeScale = 0f;

        SetActiveSafe(pausePanel, true);
        SetActiveSafe(pauseButton, false);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
            AudioManager.Instance.PauseAllGameplayAudio();
        }

        UpdateUI();
    }

    public void ResumeGame()
    {
        if (CurrentState != GameState.Paused) return;

        CurrentState = GameState.Playing;
        Time.timeScale = 1f;

        SetActiveSafe(pausePanel, false);
        SetActiveSafe(pauseButton, true);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
            AudioManager.Instance.ResumeAllGameplayAudio();
        }

        UpdateUI();
    }

    public void GameOver()
    {
        if (CurrentState == GameState.Dead) return;

        CurrentState = GameState.Dead;
        WorldSpeed = 0f;
        Time.timeScale = 1f;

        if (Score > bestScore)
        {
            bestScore = Score;
            PlayerPrefs.SetFloat(BestScoreKey, bestScore);
            PlayerPrefs.Save();
        }

        SetActiveSafe(idlePanel, false);
        SetActiveSafe(deadPanel, true);
        SetActiveSafe(pausePanel, false);
        SetActiveSafe(pauseButton, false);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayHit();
        }

        UpdateUI();
    }

    public void RestartGame()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
            AudioManager.Instance.StopAllAudio();
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClick();
            AudioManager.Instance.StopAllAudio();
        }

        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"SCORE: {Mathf.FloorToInt(Score):000000}";
        }

        if (bestScoreText != null)
        {
            bestScoreText.text = $"BEST: {Mathf.FloorToInt(bestScore):000000}";
        }
    }

    private void SetActiveSafe(GameObject target, bool active)
    {
        if (target != null)
        {
            target.SetActive(active);
        }
    }
}