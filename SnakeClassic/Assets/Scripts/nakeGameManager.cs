using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SnakeGameManager : MonoBehaviour
{


    [Header("Food Spawn Rate")]
    [SerializeField, Range(0f, 1f)] private float normalFoodRate = 0.75f;
    [SerializeField, Range(0f, 1f)] private float speedBoostFoodRate = 0.15f;
    [SerializeField, Range(0f, 1f)] private float slowFoodRate = 0.10f;

    [Header("Board Settings")]
    [SerializeField] private int boardWidth = 14;
    [SerializeField] private int boardHeight = 20;
    [SerializeField] private float cellSize = 0.5f;

    [Header("Snake Prefabs")]
    [SerializeField] private SnakeSegment headPrefab;
    [SerializeField] private SnakeSegment bodyPrefab;
    [SerializeField] private SnakeSegment tailPrefab;

    [Header("Food Prefabs")]
    [SerializeField] private Food normalFoodPrefab;
    [SerializeField] private Food speedFoodPrefab;
    [SerializeField] private Food slowFoodPrefab;

    [Header("Obstacle Prefab")]
    [SerializeField] private GameObject obstaclePrefab;

    [Header("UI Panels")]
    [SerializeField] private GameObject gameHUD;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("UI Texts")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text highScoreText;
    [SerializeField] private TMP_Text gameOverScoreText;

    [Header("Settings UI")]
    [SerializeField] private TMP_Text soundButtonText;
    [SerializeField] private TMP_Text difficultyStatusText;

    [Header("Difficulty")]
    [SerializeField] private float startMoveInterval = 0.30f;
    [SerializeField] private float minMoveInterval = 0.12f;
    [SerializeField] private int scorePerLevel = 70;
    [SerializeField] private int maxObstacles = 15;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip eatClip;
    [SerializeField] private AudioClip gameOverClip;

    private readonly List<SnakeSegment> snake = new List<SnakeSegment>();
    private readonly List<GameObject> obstacles = new List<GameObject>();

    private readonly HashSet<Vector2Int> occupiedPositions = new HashSet<Vector2Int>();
    private readonly HashSet<Vector2Int> obstaclePositions = new HashSet<Vector2Int>();

    private SwipeInput swipeInput;
    private Food currentFood;

    private Direction currentDirection = Direction.Right;
    private Direction queuedDirection = Direction.Right;

    private GameState currentState = GameState.Menu;

    private Vector2 boardOffset;

    private float moveTimer;
    private float moveInterval;

    private int score;
    private int level;
    private int highScore;
    private int difficulty;
    private bool soundEnabled;

    private const string HighScoreKey = "SnakeClassicHighScore";
    private const string DifficultyKey = "SnakeClassicDifficulty";
    private const string SoundKey = "SnakeClassicSound";

    private void Awake()
    {
        swipeInput = GetComponent<SwipeInput>();

        if (swipeInput == null)
        {
            swipeInput = gameObject.AddComponent<SwipeInput>();
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        highScore = PlayerPrefs.GetInt(HighScoreKey, 0);
        difficulty = PlayerPrefs.GetInt(DifficultyKey, 2);
        soundEnabled = PlayerPrefs.GetInt(SoundKey, 1) == 1;

        CalculateBoardOffset();
    }

    private void Start()
    {
        Time.timeScale = 1f;
        ShowMainMenu();
        UpdateUI();
        UpdateSettingsUI();
    }

    private void Update()
    {
        if (currentState != GameState.Playing)
            return;

        Direction? inputDirection = swipeInput.ReadDirection();

        if (inputDirection.HasValue && !currentDirection.IsOpposite(inputDirection.Value))
        {
            queuedDirection = inputDirection.Value;
        }

        moveTimer += Time.deltaTime;

        if (moveTimer >= moveInterval)
        {
            moveTimer = 0f;
            MoveSnake();
        }
    }

    public void StartGame()
    {
        ClearGameObjects();

        currentState = GameState.Playing;

        score = 0;
        level = 1;

        currentDirection = Direction.Right;
        queuedDirection = Direction.Right;

        moveInterval = GetStartIntervalByDifficulty();
        moveTimer = 0f;

        HideAllPanels();

        if (gameHUD != null)
            gameHUD.SetActive(true);

        SpawnInitialSnake();
        SpawnObstaclesByLevel();
        SpawnFood();

        UpdateUI();
    }

    public void PauseGame()
    {
        if (currentState != GameState.Playing)
            return;

        currentState = GameState.Paused;

        if (pausePanel != null)
            pausePanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        Debug.Log("Clicked Resume");

        Time.timeScale = 1f;
        currentState = GameState.Playing;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (gameHUD != null)
            gameHUD.SetActive(true);
    }

    public void RestartGame()
    {
        Debug.Log("Clicked Restart");

        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        StartGame();
    }

    public void BackToMenu()
    {
        Debug.Log("Clicked Back To Menu");

        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        ClearGameObjects();
        ShowMainMenu();
    }

    public void OpenSettings()
    {
        HideAllPanels();

        if (settingsPanel != null)
            settingsPanel.SetActive(true);

        UpdateSettingsUI();
    }

    public void CloseSettings()
    {
        ShowMainMenu();
    }

    public void ToggleSound()
    {
        soundEnabled = !soundEnabled;

        PlayerPrefs.SetInt(SoundKey, soundEnabled ? 1 : 0);
        PlayerPrefs.Save();

        UpdateSettingsUI();
    }

    public void SetDifficulty(int value)
    {
        difficulty = Mathf.Clamp(value, 1, 3);

        PlayerPrefs.SetInt(DifficultyKey, difficulty);
        PlayerPrefs.Save();

        UpdateSettingsUI();

        Time.timeScale = 1f;

        StartGame();
    }

    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void MoveSnake()
    {
        currentDirection = queuedDirection;

        if (snake.Count <= 0)
            return;

        Vector2Int nextHeadPosition = snake[0].GridPosition + currentDirection.ToVector2Int();

        if (IsWallCollision(nextHeadPosition))
        {
            GameOver();
            return;
        }

        if (obstaclePositions.Contains(nextHeadPosition))
        {
            GameOver();
            return;
        }

        if (IsSelfCollision(nextHeadPosition))
        {
            GameOver();
            return;
        }

        bool hasEatenFood = currentFood != null &&
                            WorldToGrid(currentFood.transform.position) == nextHeadPosition;

        Vector2Int previousPosition = snake[0].GridPosition;

        snake[0].SetGridPosition(nextHeadPosition, cellSize, boardOffset);
        RotateHead();

        occupiedPositions.Clear();
        occupiedPositions.Add(nextHeadPosition);

        for (int i = 1; i < snake.Count; i++)
        {
            Vector2Int tempPosition = snake[i].GridPosition;

            snake[i].SetGridPosition(previousPosition, cellSize, boardOffset);

            previousPosition = tempPosition;

            occupiedPositions.Add(snake[i].GridPosition);
        }

        if (hasEatenFood)
        {
            EatFood(previousPosition);
        }
    }

    private void EatFood(Vector2Int growPosition)
    {
        if (currentFood == null)
            return;

        FoodType eatenType = currentFood.Type;

        Destroy(currentFood.gameObject);
        currentFood = null;

        switch (eatenType)
        {
            case FoodType.Normal:
                score += 10;
                break;

            case FoodType.SpeedBoost:
                score += 15;
                moveInterval = Mathf.Max(minMoveInterval, moveInterval * 0.92f);
                break;

            case FoodType.Slow:
                score += 8;
                moveInterval = Mathf.Min(startMoveInterval + 0.12f, moveInterval * 1.15f);
                break;
        }

        GrowSnake(growPosition);
        PlaySound(eatClip);

        UpdateDifficulty();
        SpawnFood();
        UpdateUI();
    }

    private void GrowSnake(Vector2Int gridPosition)
    {
        if (bodyPrefab == null)
        {
            Debug.LogError("Body Prefab is missing.");
            return;
        }

        SnakeSegment newSegment = Instantiate(bodyPrefab);
        newSegment.SetGridPosition(gridPosition, cellSize, boardOffset);

        snake.Add(newSegment);
        occupiedPositions.Add(gridPosition);

        RefreshTailSprite();
    }

    private void RefreshTailSprite()
    {
        if (snake.Count < 2 || tailPrefab == null)
            return;

        int lastIndex = snake.Count - 1;

        Vector2Int tailPosition = snake[lastIndex].GridPosition;

        Destroy(snake[lastIndex].gameObject);

        SnakeSegment newTail = Instantiate(tailPrefab);
        newTail.SetGridPosition(tailPosition, cellSize, boardOffset);

        snake[lastIndex] = newTail;
    }

    private void SpawnInitialSnake()
    {
        if (headPrefab == null || bodyPrefab == null || tailPrefab == null)
        {
            Debug.LogError("Snake prefabs are missing. Please assign Head, Body, Tail prefabs.");
            return;
        }

        Vector2Int startPosition = new Vector2Int(boardWidth / 2, boardHeight / 2);

        SnakeSegment head = Instantiate(headPrefab);
        head.SetGridPosition(startPosition, cellSize, boardOffset);
        snake.Add(head);

        SnakeSegment body = Instantiate(bodyPrefab);
        body.SetGridPosition(startPosition + Vector2Int.left, cellSize, boardOffset);
        snake.Add(body);

        SnakeSegment tail = Instantiate(tailPrefab);
        tail.SetGridPosition(startPosition + Vector2Int.left * 2, cellSize, boardOffset);
        snake.Add(tail);

        occupiedPositions.Clear();

        foreach (SnakeSegment segment in snake)
        {
            occupiedPositions.Add(segment.GridPosition);
        }

        RotateHead();
    }

    private void SpawnFood()
    {
        Vector2Int position = GetRandomFreePosition();

        FoodType type = GetRandomFoodType();
        Food prefab = GetFoodPrefab(type);

        if (prefab == null)
        {
            Debug.LogError("Food prefab is missing. Please assign food prefabs.");
            return;
        }

        currentFood = Instantiate(prefab);
        currentFood.Configure(type);
        currentFood.transform.position = GridToWorld(position);
    }
    private FoodType GetRandomFoodType()
    {
        float totalRate = normalFoodRate + speedBoostFoodRate + slowFoodRate;

        if (totalRate <= 0f)
        {
            return FoodType.Normal;
        }

        float random = Random.value * totalRate;

        if (random < normalFoodRate)
        {
            return FoodType.Normal;
        }

        if (random < normalFoodRate + speedBoostFoodRate)
        {
            return FoodType.SpeedBoost;
        }

        return FoodType.Slow;
    }

    private Food GetFoodPrefab(FoodType type)
    {
        switch (type)
        {
            case FoodType.SpeedBoost:
                return speedFoodPrefab != null ? speedFoodPrefab : normalFoodPrefab;

            case FoodType.Slow:
                return slowFoodPrefab != null ? slowFoodPrefab : normalFoodPrefab;

            default:
                return normalFoodPrefab;
        }
    }

    private void SpawnObstaclesByLevel()
    {
        if (obstaclePrefab == null)
            return;

        int targetObstacleCount = GetObstacleCountByDifficulty();

        while (obstaclePositions.Count < targetObstacleCount)
        {
            Vector2Int position = GetRandomFreePosition();

            GameObject obstacle = Instantiate(obstaclePrefab);
            obstacle.transform.position = GridToWorld(position);

            obstaclePositions.Add(position);
            obstacles.Add(obstacle);
        }
    }


    private int GetObstacleCountByDifficulty()
    {
        int baseObstacleCount;

        switch (difficulty)
        {
            case 1:
                baseObstacleCount = 5;
                break;

            case 2:
                baseObstacleCount = 8;
                break;

            case 3:
                baseObstacleCount = 12;
                break;

            default:
                baseObstacleCount = 8;
                break;
        }

        int levelBonus = Mathf.Max(0, level - 1);
        int totalObstacleCount = baseObstacleCount + levelBonus;

        return Mathf.Min(maxObstacles, totalObstacleCount);
    }




    private Vector2Int GetRandomFreePosition()
    {
        for (int i = 0; i < 500; i++)
        {
            Vector2Int position = new Vector2Int(
                Random.Range(0, boardWidth),
                Random.Range(0, boardHeight)
            );

            if (!occupiedPositions.Contains(position) &&
                !obstaclePositions.Contains(position))
            {
                return position;
            }
        }

        return Vector2Int.zero;
    }

    private void UpdateDifficulty()
    {
        int newLevel = score / scorePerLevel + 1;

        if (newLevel > level)
        {
            level = newLevel;

            moveInterval = Mathf.Max(minMoveInterval, moveInterval - 0.008f);

            SpawnObstaclesByLevel();
        }
    }

    private bool IsWallCollision(Vector2Int position)
    {
        return position.x < 0 ||
               position.x >= boardWidth ||
               position.y < 0 ||
               position.y >= boardHeight;
    }

    private bool IsSelfCollision(Vector2Int position)
    {
        for (int i = 1; i < snake.Count; i++)
        {
            if (snake[i].GridPosition == position)
            {
                return true;
            }
        }

        return false;
    }

    private void GameOver()
    {
        currentState = GameState.GameOver;

        PlaySound(gameOverClip);

        if (gameHUD != null)
            gameHUD.SetActive(false);

        if (score > highScore)
        {
            highScore = score;

            PlayerPrefs.SetInt(HighScoreKey, highScore);
            PlayerPrefs.Save();
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (gameOverScoreText != null)
            gameOverScoreText.text = "Điểm: " + score;

        UpdateUI();
    }

    private void ShowMainMenu()
    {
        currentState = GameState.Menu;

        HideAllPanels();

        if (gameHUD != null)
            gameHUD.SetActive(false);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        UpdateUI();
    }

    private void HideAllPanels()
    {
        if (gameHUD != null)
            gameHUD.SetActive(false);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    private void ClearGameObjects()
    {
        foreach (SnakeSegment segment in snake)
        {
            if (segment != null)
                Destroy(segment.gameObject);
        }

        snake.Clear();
        occupiedPositions.Clear();

        foreach (GameObject obstacle in obstacles)
        {
            if (obstacle != null)
                Destroy(obstacle);
        }

        obstacles.Clear();
        obstaclePositions.Clear();

        if (currentFood != null)
        {
            Destroy(currentFood.gameObject);
            currentFood = null;
        }
    }

    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Điểm: " + score;

        if (levelText != null)
            levelText.text = "Level: " + level;

        if (highScoreText != null)
            highScoreText.text = "Điểm cao: " + highScore;
    }

    private void RotateHead()
    {
        if (snake.Count <= 0)
            return;

        float angle = 0f;

        switch (currentDirection)
        {
            case Direction.Up:
                angle = 0f;
                break;

            case Direction.Right:
                angle = -90f;
                break;

            case Direction.Down:
                angle = 180f;
                break;

            case Direction.Left:
                angle = 90f;
                break;
        }

        snake[0].transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void CalculateBoardOffset()
    {
        float offsetX = -(boardWidth - 1) * cellSize * 0.5f;
        float offsetY = -(boardHeight - 1) * cellSize * 0.5f;

        boardOffset = new Vector2(offsetX, offsetY);
    }

    private Vector3 GridToWorld(Vector2Int gridPosition)
    {
        return new Vector3(
            boardOffset.x + gridPosition.x * cellSize,
            boardOffset.y + gridPosition.y * cellSize,
            0f
        );
    }

    private Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        int x = Mathf.RoundToInt((worldPosition.x - boardOffset.x) / cellSize);
        int y = Mathf.RoundToInt((worldPosition.y - boardOffset.y) / cellSize);

        return new Vector2Int(x, y);
    }

    private float GetStartIntervalByDifficulty()
    {
        switch (difficulty)
        {
            case 1:
                return startMoveInterval + 0.1f;

            case 2:
                return startMoveInterval;

            case 3:
                return Mathf.Max(minMoveInterval, startMoveInterval - 0.007f);

            default:
                return startMoveInterval;
        }
    }

    private void UpdateSettingsUI()
    {
        if (soundButtonText != null)
        {
            soundButtonText.text = soundEnabled ? "ÂM THANH: BẬT" : "ÂM THANH: TẮT";
        }

        if (difficultyStatusText != null)
        {
            difficultyStatusText.text = "ĐỘ KHÓ HIỆN TẠI: " + GetDifficultyName();
        }
    }

    private string GetDifficultyName()
    {
        switch (difficulty)
        {
            case 1:
                return "DỄ";

            case 2:
                return "THƯỜNG";

            case 3:
                return "KHÓ";

            default:
                return "THƯỜNG";
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (!soundEnabled)
            return;

        if (audioSource == null)
            return;

        if (clip == null)
            return;

        audioSource.PlayOneShot(clip);
    }
}