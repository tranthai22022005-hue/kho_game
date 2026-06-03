using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    [Header("Tilemaps")]
    [SerializeField] private Tilemap boardTilemap;
    [SerializeField] private Tilemap ghostTilemap;
    [SerializeField] private TileBase ghostTile;

    [Header("Pieces")]
    [SerializeField] private Piece activePiece;
    [SerializeField] private TetrominoData[] tetrominoes;

    [Header("Board")]
    [SerializeField] private Vector2Int boardSize = new Vector2Int(10, 20);

    // FIX QUAN TRỌNG:
    // Vì Bounds của bảng đang nằm giữa tâm:
    // X: -5 -> 4, Y: -10 -> 9
    // nên spawn phải nằm trong vùng đó.
    [SerializeField] private Vector3Int spawnPosition = new Vector3Int(0, 8, 0);

    [Header("Timing")]
    [SerializeField] private float baseStepDelay = 1f;
    [SerializeField] private float minimumStepDelay = 0.08f;
    [SerializeField] private float softDropStepDelay = 0.04f;
    [SerializeField] private float lockDelay = 0.5f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI linesText;
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Tilemap holdTilemap;
    [SerializeField] private Tilemap[] nextTilemaps;

    [Header("Audio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip moveSfx;
    [SerializeField] private AudioClip rotateSfx;
    [SerializeField] private AudioClip softDropSfx;
    [SerializeField] private AudioClip hardDropSfx;
    [SerializeField] private AudioClip lineClearSfx;
    [SerializeField] private AudioClip tetrisClearSfx;
    [SerializeField] private AudioClip holdSfx;
    [SerializeField] private AudioClip levelUpSfx;
    [SerializeField] private AudioClip gameOverSfx;

    private readonly Queue<Tetromino> nextQueue = new Queue<Tetromino>();
    private readonly TetrisBag bag = new TetrisBag();
    private Tetromino? holdPiece;
    private bool canHold = true;

    private int score;
    private int level = 1;
    private int lines;
    private int combo = -1;

    public GameState State { get; private set; } = GameState.Ready;
    public float SoftDropStepDelay => softDropStepDelay;

    public RectInt Bounds
    {
        get
        {
            Vector2Int position = new Vector2Int(-boardSize.x / 2, -boardSize.y / 2);
            return new RectInt(position, boardSize);
        }
    }

    private void Awake()
    {
        for (int i = 0; i < tetrominoes.Length; i++)
            tetrominoes[i].Initialize();
    }

    private void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        boardTilemap.ClearAllTiles();
        ghostTilemap.ClearAllTiles();

        if (holdTilemap != null)
            holdTilemap.ClearAllTiles();

        if (nextTilemaps != null)
        {
            for (int i = 0; i < nextTilemaps.Length; i++)
            {
                if (nextTilemaps[i] != null)
                    nextTilemaps[i].ClearAllTiles();
            }
        }

        score = 0;
        level = 1;
        lines = 0;
        combo = -1;
        holdPiece = null;
        canHold = true;
        nextQueue.Clear();

        while (nextQueue.Count < 3)
            nextQueue.Enqueue(bag.Next());

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        State = GameState.Playing;
        SpawnPiece();
        UpdateUI();

        if (musicSource != null)
        {
            musicSource.Stop();
            musicSource.Play();
        }
    }

    public void TogglePause()
    {
        if (State == GameState.Playing)
        {
            State = GameState.Paused;
            Time.timeScale = 0f;
        }
        else if (State == GameState.Paused)
        {
            Time.timeScale = 1f;
            State = GameState.Playing;
        }
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        StartGame();
    }

    public void SpawnPiece()
    {
        Tetromino next = nextQueue.Dequeue();
        nextQueue.Enqueue(bag.Next());

        TetrominoData data = GetData(next);
        float currentStepDelay = Mathf.Max(minimumStepDelay, baseStepDelay - ((level - 1) * 0.075f));

        activePiece.Initialize(this, spawnPosition, data, currentStepDelay, lockDelay);

        if (!IsValidPosition(activePiece, spawnPosition))
        {
            GameOver();
            return;
        }

        Set(activePiece);
        DrawGhost(activePiece);
        DrawHoldAndNext();
    }

    private TetrominoData GetData(Tetromino tetromino)
    {
        for (int i = 0; i < tetrominoes.Length; i++)
        {
            if (tetrominoes[i].tetromino == tetromino)
                return tetrominoes[i];
        }

        return tetrominoes[0];
    }

    public void Hold()
    {
        if (!canHold || State != GameState.Playing) return;

        Clear(activePiece);
        Tetromino current = activePiece.data.tetromino;

        if (holdPiece.HasValue)
        {
            Tetromino swap = holdPiece.Value;
            holdPiece = current;

            activePiece.Initialize(
                this,
                spawnPosition,
                GetData(swap),
                Mathf.Max(minimumStepDelay, baseStepDelay - ((level - 1) * 0.075f)),
                lockDelay
            );

            if (!IsValidPosition(activePiece, spawnPosition))
            {
                GameOver();
                return;
            }

            Set(activePiece);
            DrawGhost(activePiece);
        }
        else
        {
            holdPiece = current;
            SpawnPiece();
        }

        canHold = false;
        DrawHoldAndNext();
        PlayClip(holdSfx);
    }

    public void Lock(Piece piece)
    {
        Set(piece);

        bool tSpin = IsTSpin(piece);
        int cleared = ClearLines();

        if (cleared > 0)
        {
            combo++;
            int lineScore = CalculateLineScore(cleared, tSpin);
            int comboBonus = combo > 0 ? combo * 50 * level : 0;
            score += lineScore + comboBonus;

            if (cleared == 4)
                PlayClip(tetrisClearSfx);
            else
                PlayClip(lineClearSfx);
        }
        else
        {
            combo = -1;
        }

        canHold = true;
        SpawnPiece();
        UpdateUI();
    }

    private int CalculateLineScore(int cleared, bool tSpin)
    {
        if (tSpin)
        {
            if (cleared == 1) return 800 * level;
            if (cleared == 2) return 1200 * level;
            if (cleared == 3) return 1600 * level;
            return 400 * level;
        }

        switch (cleared)
        {
            case 1: return 100 * level;
            case 2: return 300 * level;
            case 3: return 500 * level;
            case 4: return 800 * level;
            default: return 0;
        }
    }

    private bool IsTSpin(Piece piece)
    {
        if (piece.data.tetromino != Tetromino.T || !piece.LastMoveWasRotation)
            return false;

        Vector3Int center = piece.position;
        Vector3Int[] corners =
        {
            center + new Vector3Int(-1, -1, 0),
            center + new Vector3Int( 1, -1, 0),
            center + new Vector3Int(-1,  1, 0),
            center + new Vector3Int( 1,  1, 0)
        };

        int blocked = 0;
        for (int i = 0; i < corners.Length; i++)
        {
            if (!Bounds.Contains((Vector2Int)corners[i]) || boardTilemap.HasTile(corners[i]))
                blocked++;
        }

        return blocked >= 3;
    }

    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            boardTilemap.SetTile(tilePosition, piece.data.tile);
        }
    }

    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            boardTilemap.SetTile(tilePosition, null);
        }
    }

    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        RectInt bounds = Bounds;

        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + position;

            if (!bounds.Contains((Vector2Int)tilePosition))
                return false;

            if (boardTilemap.HasTile(tilePosition))
                return false;
        }

        return true;
    }

    private int ClearLines()
    {
        RectInt bounds = Bounds;
        int row = bounds.yMin;
        int cleared = 0;

        while (row < bounds.yMax)
        {
            if (IsLineFull(row))
            {
                LineClear(row);
                cleared++;
            }
            else
            {
                row++;
            }
        }

        if (cleared > 0)
        {
            lines += cleared;
            int newLevel = 1 + lines / 10;

            if (newLevel > level)
            {
                level = newLevel;
                PlayClip(levelUpSfx);
            }
        }

        return cleared;
    }

    private bool IsLineFull(int row)
    {
        RectInt bounds = Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            if (!boardTilemap.HasTile(position))
                return false;
        }

        return true;
    }

    private void LineClear(int row)
    {
        RectInt bounds = Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
            boardTilemap.SetTile(new Vector3Int(col, row, 0), null);

        while (row < bounds.yMax)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int positionAbove = new Vector3Int(col, row + 1, 0);
                TileBase above = boardTilemap.GetTile(positionAbove);

                Vector3Int currentPosition = new Vector3Int(col, row, 0);
                boardTilemap.SetTile(currentPosition, above);
            }
            row++;
        }
    }

    public void DrawGhost(Piece piece)
    {
        ghostTilemap.ClearAllTiles();

        Vector3Int ghostPosition = piece.position;
        while (IsValidPosition(piece, ghostPosition + Vector3Int.down))
            ghostPosition += Vector3Int.down;

        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + ghostPosition;
            ghostTilemap.SetTile(tilePosition, ghostTile);
        }
    }

    private void DrawHoldAndNext()
    {
        if (holdTilemap != null)
        {
            holdTilemap.ClearAllTiles();
            if (holdPiece.HasValue)
                DrawPreview(holdTilemap, GetData(holdPiece.Value));
        }

        if (nextTilemaps != null)
        {
            Tetromino[] queue = nextQueue.ToArray();

            for (int i = 0; i < nextTilemaps.Length; i++)
            {
                if (nextTilemaps[i] == null) continue;

                nextTilemaps[i].ClearAllTiles();

                if (i < queue.Length)
                    DrawPreview(nextTilemaps[i], GetData(queue[i]));
            }
        }
    }

    private void DrawPreview(Tilemap tilemap, TetrominoData data)
    {
        for (int i = 0; i < data.cells.Length; i++)
        {
            Vector3Int pos = (Vector3Int)data.cells[i];
            tilemap.SetTile(pos, data.tile);
        }
    }

    public void AddSoftDropScore(int amount)
    {
        score += amount;
        PlayClip(softDropSfx);
        UpdateUI();
    }

    public void AddHardDropScore(int cellsDropped)
    {
        score += cellsDropped * 2;
        UpdateUI();
    }

    private void GameOver()
    {
        State = GameState.GameOver;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        PlayClip(gameOverSfx);

        if (musicSource != null)
            musicSource.Stop();
    }

    private void UpdateUI()
    {
        if (scoreText != null) scoreText.text = score.ToString("000000");
        if (levelText != null) levelText.text = "LEVEL " + level;
        if (linesText != null) linesText.text = "LINES " + lines;
        if (comboText != null) comboText.text = combo > 0 ? "COMBO x" + combo : "";
    }

    public void PlayMoveSfx() => PlayClip(moveSfx);
    public void PlayRotateSfx() => PlayClip(rotateSfx);
    public void PlayHardDropSfx() => PlayClip(hardDropSfx);

    private void PlayClip(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
            sfxSource.PlayOneShot(clip);
    }
}