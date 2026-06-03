using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

// Game bắn bóng 2D thuần Canvas/Sprite UI - phiên bản Fantasy Lake.
// Class vẫn tên XepHinhGame để scene có sẵn nhận đúng script.
public class XepHinhGame : MonoBehaviour
{
    private const string BestScoreKey = "BAN_BONG_FANTASY_LAKE_BEST_SCORE";
    private const int Columns = 9;
    private const int StartRows = 5;
    private const int MaxRows = 12;
    private const float BubbleSize = 62f;
    private const float CellX = 66f;
    private const float CellY = 57f;
    private const float ShotSpeed = 980f;

    private readonly Color32[] tintColors =
    {
        new Color32(255,255,255,255),
        new Color32(255,255,255,255),
        new Color32(255,255,255,255),
        new Color32(255,255,255,255),
        new Color32(255,255,255,255),
        new Color32(255,255,255,255)
    };

    private Canvas canvas;
    private Font font;
    private GameObject menuRoot;
    private GameObject gameRoot;
    private GameObject pauseOverlay;
    private GameObject settingOverlay;
    private GameObject gameOverRoot;

    private RectTransform playRoot;
    private RectTransform cannonRoot;
    private RectTransform aimRoot;
    private RectTransform nextRoot;
    private Text scoreText;
    private Text bestText;
    private Text finalScoreText;
    private Text musicStatusText;

    private AudioSource sfxSource;
    private AudioSource musicSource;

    private Sprite menuBackground;
    private Sprite gameBackground;
    private Sprite buttonPlay;
    private Sprite buttonSetting;
    private Sprite buttonExit;
    private Sprite buttonRestart;
    private Sprite buttonLeft;
    private Sprite buttonRight;
    private Sprite buttonShoot;
    private Sprite buttonSwap;
    private Sprite buttonPause;
    private Sprite scorePanel;
    private Sprite bestPanel;
    private Sprite boardFrame;
    private Sprite nextPanel;
    private Sprite cannonSprite;
    private Sprite aimDotsSprite;
    private Sprite popSprite;
    private Sprite[] bubbleSprites;

    private AudioClip bgmClip;
    private AudioClip clickClip;
    private AudioClip shootClip;
    private AudioClip popClip;
    private AudioClip gameOverClip;
    private AudioClip winClip;

    private readonly Dictionary<Vector2Int, BubbleNode> grid = new Dictionary<Vector2Int, BubbleNode>();
    private readonly List<Image> temporaryImages = new List<Image>();
    private readonly System.Random random = new System.Random();

    private int score;
    private int bestScore;
    private int currentColor;
    private int nextColor;
    private float aimAngle;
    private bool isPlaying;
    private bool isPaused;
    private bool isGameOver;
    private bool isShooting;
    private bool musicEnabled = true;
    private Image currentBubbleImage;
    private Vector2 currentBubblePosition;
    private Vector2 currentBubbleDirection;

    private class BubbleNode
    {
        public Vector2Int cell;
        public int color;
        public Image image;
    }

    private void Awake()
    {
        Application.targetFrameRate = 60;
        LoadResources();
        CreateCanvas();
        CreateEventSystemIfNeeded();
        CreateAudio();
        CreateMenu();
        CreateGame();
        CreateGameOver();
        ShowMenu();
    }

    private void Update()
    {
        if (!isPlaying || isPaused || isGameOver) return;

        HandleKeyboardInput();
        UpdateAimVisual();

        if (isShooting)
        {
            UpdateShot(Time.deltaTime);
        }
    }

    private void LoadResources()
    {
        string root = "UI/FantasyLake/";
        menuBackground = LoadTextureAsSprite(root + "Backgrounds/menu_background");
        gameBackground = LoadTextureAsSprite(root + "Backgrounds/background_fantasy_lake");
        buttonPlay = LoadTextureAsSprite(root + "Buttons/button_play");
        buttonSetting = LoadTextureAsSprite(root + "Buttons/button_settings");
        buttonExit = LoadTextureAsSprite(root + "Buttons/button_exit");
        buttonRestart = LoadTextureAsSprite(root + "Buttons/button_restart");
        buttonLeft = LoadTextureAsSprite(root + "Buttons/button_left");
        buttonRight = LoadTextureAsSprite(root + "Buttons/button_right");
        buttonShoot = LoadTextureAsSprite(root + "Buttons/button_down");
        buttonSwap = LoadTextureAsSprite(root + "Buttons/button_rotate");
        buttonPause = LoadTextureAsSprite(root + "Buttons/button_pause");
        bestPanel = LoadTextureAsSprite(root + "Panels/best_panel_blank");
        scorePanel = LoadTextureAsSprite(root + "Panels/score_panel_blank");
        boardFrame = LoadTextureAsSprite(root + "Panels/board_frame_grid_transparent");
        nextPanel = LoadTextureAsSprite(root + "Panels/next_panel_blank");
        cannonSprite = LoadTextureAsSprite(root + "TransparentSprites/Cannon/bubble_cannon");
        aimDotsSprite = LoadTextureAsSprite(root + "TransparentSprites/Cannon/aim_dots");
        popSprite = LoadTextureAsSprite(root + "TransparentSprites/Effects/pop_burst");
        bubbleSprites = new[]
        {
            LoadTextureAsSprite(root + "TransparentSprites/Bubbles/bubble_blue"),
            LoadTextureAsSprite(root + "TransparentSprites/Bubbles/bubble_green"),
            LoadTextureAsSprite(root + "TransparentSprites/Bubbles/bubble_orange"),
            LoadTextureAsSprite(root + "TransparentSprites/Bubbles/bubble_purple"),
            LoadTextureAsSprite(root + "TransparentSprites/Bubbles/bubble_red"),
            LoadTextureAsSprite(root + "TransparentSprites/Bubbles/bubble_yellow")
        };

        bgmClip = Resources.Load<AudioClip>("Audio/bgm_menu");
        clickClip = Resources.Load<AudioClip>("Audio/click");
        shootClip = Resources.Load<AudioClip>("Audio/shoot");
        popClip = Resources.Load<AudioClip>("Audio/pop");
        gameOverClip = Resources.Load<AudioClip>("Audio/gameover");
        winClip = Resources.Load<AudioClip>("Audio/win");
    }

    private Sprite LoadTextureAsSprite(string path)
    {
        Texture2D texture = Resources.Load<Texture2D>(path);
        if (texture == null)
        {
            Debug.LogWarning("Missing texture: " + path);
            return null;
        }
        texture.filterMode = FilterMode.Bilinear;
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
    }

    private void CreateCanvas()
    {
        GameObject canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
    }

    private void CreateEventSystemIfNeeded()
    {
        EventSystem existing = FindObjectOfType<EventSystem>();
        if (existing == null)
        {
            existing = new GameObject("EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();
        }

        BaseInputModule[] modules = existing.GetComponents<BaseInputModule>();
        foreach (BaseInputModule module in modules)
        {
            if (module != null) Destroy(module);
        }

#if ENABLE_INPUT_SYSTEM
        existing.gameObject.AddComponent<InputSystemUIInputModule>();
#else
        existing.gameObject.AddComponent<StandaloneInputModule>();
#endif
    }

    private void CreateAudio()
    {
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.volume = 0.8f;
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.clip = bgmClip;
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = 0.32f;
        if (bgmClip != null) musicSource.Play();
    }

    private void CreateMenu()
    {
        menuRoot = CreateFullScreenRoot("MenuUI", menuBackground);
        CreateText(menuRoot.transform, "BẮN BÓNG", 0, 420, 92, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold, new Vector2(820, 130));
        CreateText(menuRoot.transform, "Fantasy Lake 2D", 0, 330, 42, TextAnchor.MiddleCenter, new Color(0.85f, 0.96f, 1f), FontStyle.Bold, new Vector2(650, 70));
        CreateImageButton(menuRoot.transform, "PlayButton", buttonPlay, new Vector2(560, 170), new Vector2(0, 80), StartGame);
        CreateImageButton(menuRoot.transform, "SettingButton", buttonSetting, new Vector2(540, 164), new Vector2(0, -105), ToggleSettings);
        CreateImageButton(menuRoot.transform, "ExitButton", buttonExit, new Vector2(520, 158), new Vector2(0, -285), Application.Quit);
        CreateText(menuRoot.transform, "Điều khiển: ← → để ngắm, ↓ để bắn, nút xoay để đổi bóng", 0, -730, 34, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold, new Vector2(980, 80));

        settingOverlay = CreateImage(menuRoot.transform, "SettingOverlay", scorePanel, new Vector2(760, 450), Vector2.zero, Image.Type.Simple);
        CreateText(settingOverlay.transform, "CÀI ĐẶT", 0, 135, 58, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold, new Vector2(650, 90));
        musicStatusText = CreateText(settingOverlay.transform, "Nhạc chờ: BẬT", 0, 35, 42, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold, new Vector2(650, 75));
        CreateTextButton(settingOverlay.transform, "BẬT / TẮT NHẠC", 0, -70, 460, 90, ToggleMusic, 36);
        CreateTextButton(settingOverlay.transform, "ĐÓNG", 0, -180, 360, 86, ToggleSettings, 36);
        settingOverlay.SetActive(false);
    }

    private void CreateGame()
    {
        gameRoot = CreateFullScreenRoot("GameUI", gameBackground);

        GameObject bestObj = CreateImage(gameRoot.transform, "BestPanel", bestPanel, new Vector2(285, 115), new Vector2(-360, 780), Image.Type.Simple);
        CreateText(bestObj.transform, "BEST", 0, 25, 28, TextAnchor.MiddleCenter, new Color(1f, 0.92f, 0.18f), FontStyle.Bold, new Vector2(210, 40));
        bestText = CreateText(bestObj.transform, "0000", 0, -24, 34, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold, new Vector2(210, 52));

        GameObject scoreObj = CreateImage(gameRoot.transform, "ScorePanel", scorePanel, new Vector2(390, 112), new Vector2(0, 780), Image.Type.Simple);
        CreateText(scoreObj.transform, "SCORE", 0, 29, 28, TextAnchor.MiddleCenter, new Color(1f, 0.92f, 0.18f), FontStyle.Bold, new Vector2(300, 42));
        scoreText = CreateText(scoreObj.transform, "0000", 0, -20, 52, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold, new Vector2(330, 66));

        CreateImageButton(gameRoot.transform, "PauseButton", buttonPause, new Vector2(126, 126), new Vector2(390, 780), TogglePause);

        GameObject boardObj = CreateImage(gameRoot.transform, "PlayFrame", boardFrame, new Vector2(760, 1090), new Vector2(-42, 75), Image.Type.Simple);
        playRoot = CreateRect(boardObj.transform, "BubblePlayRoot", new Vector2(630, 890), new Vector2(0, 42), new Vector2(0.5f, 0.5f));

        aimRoot = CreateImage(playRoot, "AimDots", aimDotsSprite, new Vector2(42, 420), new Vector2(0, -350), Image.Type.Simple).GetComponent<RectTransform>();
        aimRoot.pivot = new Vector2(0.5f, 0f);
        cannonRoot = CreateImage(playRoot, "Cannon", cannonSprite, new Vector2(145, 165), new Vector2(0, -405), Image.Type.Simple).GetComponent<RectTransform>();

        GameObject nextObj = CreateImage(gameRoot.transform, "NextPanel", nextPanel, new Vector2(205, 350), new Vector2(386, 280), Image.Type.Simple);
        CreateText(nextObj.transform, "NEXT", 0, 112, 32, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold, new Vector2(150, 44));
        nextRoot = CreateRect(nextObj.transform, "NextBubbleRoot", new Vector2(100, 100), new Vector2(0, -28), new Vector2(0.5f, 0.5f));

        CreateImageButton(gameRoot.transform, "LeftButton", buttonLeft, new Vector2(172, 172), new Vector2(-360, -780), () => AdjustAim(-8f));
        CreateImageButton(gameRoot.transform, "ShootButton", buttonShoot, new Vector2(172, 172), new Vector2(-120, -780), ShootBubble);
        CreateImageButton(gameRoot.transform, "RightButton", buttonRight, new Vector2(172, 172), new Vector2(120, -780), () => AdjustAim(8f));
        CreateImageButton(gameRoot.transform, "SwapButton", buttonSwap, new Vector2(172, 172), new Vector2(360, -780), SwapBubble);

        pauseOverlay = CreateImage(gameRoot.transform, "PauseOverlay", scorePanel, new Vector2(760, 440), Vector2.zero, Image.Type.Simple);
        CreateText(pauseOverlay.transform, "TẠM DỪNG", 0, 120, 62, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold, new Vector2(650, 90));
        CreateTextButton(pauseOverlay.transform, "TIẾP TỤC", 0, 10, 430, 94, TogglePause, 40);
        CreateTextButton(pauseOverlay.transform, "VỀ MENU", 0, -115, 430, 94, ShowMenu, 40);
        pauseOverlay.SetActive(false);
    }

    private void CreateGameOver()
    {
        gameOverRoot = CreateFullScreenRoot("GameOverUI", menuBackground);
        CreateText(gameOverRoot.transform, "GAME OVER", 0, 390, 88, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold, new Vector2(850, 130));
        finalScoreText = CreateText(gameOverRoot.transform, "SCORE: 0000\nBEST: 0000", 0, 185, 54, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold, new Vector2(760, 180));
        CreateImageButton(gameOverRoot.transform, "ReplayButton", buttonRestart, new Vector2(560, 170), new Vector2(0, -70), StartGame);
        CreateImageButton(gameOverRoot.transform, "MenuButton", buttonExit, new Vector2(520, 158), new Vector2(0, -260), ShowMenu);
    }

    private GameObject CreateFullScreenRoot(string name, Sprite background)
    {
        GameObject root = new GameObject(name, typeof(RectTransform), typeof(Image));
        root.transform.SetParent(canvas.transform, false);
        RectTransform rt = root.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        Image image = root.GetComponent<Image>();
        image.sprite = background;
        image.preserveAspect = false;
        image.raycastTarget = false;
        return root;
    }

    private GameObject CreateImage(Transform parent, string name, Sprite sprite, Vector2 size, Vector2 position, Image.Type type)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image));
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = position;
        Image image = obj.GetComponent<Image>();
        image.sprite = sprite;
        image.type = type;
        image.preserveAspect = false;
        image.raycastTarget = false;
        return obj;
    }

    private RectTransform CreateRect(Transform parent, string name, Vector2 size, Vector2 position, Vector2 anchor)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = position;
        return rt;
    }

    private Text CreateText(Transform parent, string text, float x, float y, int size, TextAnchor anchor, Color color, FontStyle style, Vector2 rectSize)
    {
        GameObject obj = new GameObject("Text", typeof(RectTransform), typeof(Text), typeof(Outline));
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = rectSize;
        rt.anchoredPosition = new Vector2(x, y);
        Text label = obj.GetComponent<Text>();
        label.text = text;
        label.font = font;
        label.fontSize = size;
        label.alignment = anchor;
        label.color = color;
        label.fontStyle = style;
        label.resizeTextForBestFit = true;
        label.resizeTextMinSize = 14;
        label.resizeTextMaxSize = size;
        label.raycastTarget = false;
        Outline outline = obj.GetComponent<Outline>();
        outline.effectColor = new Color(0.02f, 0.06f, 0.2f, 0.95f);
        outline.effectDistance = new Vector2(3, -3);
        return label;
    }

    private Button CreateImageButton(Transform parent, string name, Sprite sprite, Vector2 size, Vector2 position, UnityEngine.Events.UnityAction callback)
    {
        GameObject obj = CreateImage(parent, name, sprite, size, position, Image.Type.Simple);
        Image image = obj.GetComponent<Image>();
        image.raycastTarget = true;
        Button button = obj.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(() =>
        {
            Play(clickClip);
            callback?.Invoke();
        });
        return button;
    }

    private Button CreateTextButton(Transform parent, string label, float x, float y, float width, float height, UnityEngine.Events.UnityAction callback, int textSize)
    {
        GameObject obj = CreateImage(parent, label + "Button", scorePanel, new Vector2(width, height), new Vector2(x, y), Image.Type.Simple);
        Image image = obj.GetComponent<Image>();
        image.raycastTarget = true;
        Button button = obj.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(() =>
        {
            Play(clickClip);
            callback?.Invoke();
        });
        CreateText(obj.transform, label, 0, 0, textSize, TextAnchor.MiddleCenter, Color.white, FontStyle.Bold, new Vector2(width * 0.9f, height * 0.82f));
        return button;
    }

    private void StartGame()
    {
        ClearAllBubbles();
        score = 0;
        bestScore = PlayerPrefs.GetInt(BestScoreKey, 0);
        currentColor = RandomColor();
        nextColor = RandomColor();
        aimAngle = 0f;
        isPlaying = true;
        isPaused = false;
        isGameOver = false;
        isShooting = false;
        ShowOnly(gameRoot);
        pauseOverlay.SetActive(false);
        BuildInitialGrid();
        UpdateNextBubble();
        UpdateScoreUI();
        DrawCurrentBubbleAtCannon();
    }

    private void ShowMenu()
    {
        isPlaying = false;
        isPaused = false;
        isGameOver = false;
        if (pauseOverlay != null) pauseOverlay.SetActive(false);
        if (settingOverlay != null) settingOverlay.SetActive(false);
        ShowOnly(menuRoot);
    }

    private void ShowOnly(GameObject root)
    {
        if (menuRoot != null) menuRoot.SetActive(root == menuRoot);
        if (gameRoot != null) gameRoot.SetActive(root == gameRoot);
        if (gameOverRoot != null) gameOverRoot.SetActive(root == gameOverRoot);
    }

    private void BuildInitialGrid()
    {
        for (int r = 0; r < StartRows; r++)
        {
            for (int c = 0; c < Columns; c++)
            {
                if (r % 2 == 1 && c == Columns - 1) continue;
                AddBubble(new Vector2Int(c, r), RandomColor());
            }
        }
    }

    private int RandomColor()
    {
        return random.Next(bubbleSprites.Length);
    }

    private Vector2 CellToPosition(Vector2Int cell)
    {
        float x = (cell.x - (Columns - 1) * 0.5f) * CellX;
        if (cell.y % 2 == 1) x += CellX * 0.5f;
        float y = 360f - cell.y * CellY;
        return new Vector2(x, y);
    }

    private Vector2Int PositionToNearestCell(Vector2 position)
    {
        int bestC = 0;
        int bestR = 0;
        float bestDistance = float.MaxValue;
        for (int r = 0; r < MaxRows; r++)
        {
            int maxC = r % 2 == 1 ? Columns - 1 : Columns;
            for (int c = 0; c < maxC; c++)
            {
                Vector2Int cell = new Vector2Int(c, r);
                if (grid.ContainsKey(cell)) continue;
                float d = Vector2.SqrMagnitude(CellToPosition(cell) - position);
                if (d < bestDistance)
                {
                    bestDistance = d;
                    bestC = c;
                    bestR = r;
                }
            }
        }
        return new Vector2Int(bestC, bestR);
    }

    private void AddBubble(Vector2Int cell, int color)
    {
        if (grid.ContainsKey(cell)) return;
        GameObject obj = CreateImage(playRoot, "Bubble", bubbleSprites[color], new Vector2(BubbleSize, BubbleSize), CellToPosition(cell), Image.Type.Simple);
        Image img = obj.GetComponent<Image>();
        img.color = tintColors[color % tintColors.Length];
        grid[cell] = new BubbleNode { cell = cell, color = color, image = img };
    }

    private void DrawCurrentBubbleAtCannon()
    {
        if (currentBubbleImage != null) Destroy(currentBubbleImage.gameObject);
        currentBubblePosition = new Vector2(0, -335f);
        GameObject obj = CreateImage(playRoot, "CurrentBubble", bubbleSprites[currentColor], new Vector2(BubbleSize, BubbleSize), currentBubblePosition, Image.Type.Simple);
        currentBubbleImage = obj.GetComponent<Image>();
    }

    private void UpdateNextBubble()
    {
        foreach (Image image in temporaryImages)
        {
            if (image != null) Destroy(image.gameObject);
        }
        temporaryImages.Clear();
        GameObject obj = CreateImage(nextRoot, "NextBubble", bubbleSprites[nextColor], new Vector2(76, 76), Vector2.zero, Image.Type.Simple);
        temporaryImages.Add(obj.GetComponent<Image>());
    }

    private void AdjustAim(float delta)
    {
        if (!CanControl() || isShooting) return;
        aimAngle = Mathf.Clamp(aimAngle + delta, -62f, 62f);
        UpdateAimVisual();
    }

    private void UpdateAimVisual()
    {
        if (aimRoot != null)
        {
            aimRoot.anchoredPosition = new Vector2(0, -350f);
            aimRoot.localEulerAngles = new Vector3(0, 0, -aimAngle);
        }
        if (cannonRoot != null)
        {
            cannonRoot.localEulerAngles = new Vector3(0, 0, -aimAngle);
        }
    }

    private void ShootBubble()
    {
        if (!CanControl() || isShooting) return;
        isShooting = true;
        float radians = aimAngle * Mathf.Deg2Rad;
        currentBubbleDirection = new Vector2(Mathf.Sin(radians), Mathf.Cos(radians)).normalized;
        Play(shootClip);
    }

    private void SwapBubble()
    {
        if (!CanControl() || isShooting) return;
        int temp = currentColor;
        currentColor = nextColor;
        nextColor = temp;
        DrawCurrentBubbleAtCannon();
        UpdateNextBubble();
    }

    private void UpdateShot(float deltaTime)
    {
        currentBubblePosition += currentBubbleDirection * ShotSpeed * deltaTime;

        float halfWidth = playRoot.rect.width * 0.5f - BubbleSize * 0.48f;
        if (currentBubblePosition.x < -halfWidth)
        {
            currentBubblePosition.x = -halfWidth;
            currentBubbleDirection.x *= -1f;
        }
        else if (currentBubblePosition.x > halfWidth)
        {
            currentBubblePosition.x = halfWidth;
            currentBubbleDirection.x *= -1f;
        }

        if (currentBubbleImage != null) currentBubbleImage.rectTransform.anchoredPosition = currentBubblePosition;

        if (currentBubblePosition.y >= 370f || HasBubbleCollision(currentBubblePosition))
        {
            AttachCurrentBubble();
        }
    }

    private bool HasBubbleCollision(Vector2 position)
    {
        float minDistance = BubbleSize * 0.82f;
        foreach (BubbleNode node in grid.Values)
        {
            if (Vector2.Distance(position, CellToPosition(node.cell)) <= minDistance) return true;
        }
        return false;
    }

    private void AttachCurrentBubble()
    {
        isShooting = false;
        Vector2Int cell = PositionToNearestCell(currentBubblePosition);
        if (currentBubbleImage != null) Destroy(currentBubbleImage.gameObject);
        AddBubble(cell, currentColor);
        ResolveMatches(cell);
        if (grid.Count == 0)
        {
            AddScore(500);
            Play(winClip);
            BuildInitialGrid();
        }
        if (IsGameOver())
        {
            EndGame();
            return;
        }
        currentColor = nextColor;
        nextColor = RandomColor();
        UpdateNextBubble();
        DrawCurrentBubbleAtCannon();
    }

    private void ResolveMatches(Vector2Int start)
    {
        if (!grid.ContainsKey(start)) return;
        int color = grid[start].color;
        List<Vector2Int> connected = GetConnectedSameColor(start, color);
        if (connected.Count < 3) return;

        foreach (Vector2Int cell in connected)
        {
            if (!grid.ContainsKey(cell)) continue;
            ShowPop(CellToPosition(cell));
            Destroy(grid[cell].image.gameObject);
            grid.Remove(cell);
        }
        AddScore(connected.Count * 10);
        Play(popClip);
        RemoveFloatingBubbles();
    }

    private List<Vector2Int> GetConnectedSameColor(Vector2Int start, int color)
    {
        List<Vector2Int> result = new List<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        queue.Enqueue(start);
        visited.Add(start);
        while (queue.Count > 0)
        {
            Vector2Int cell = queue.Dequeue();
            result.Add(cell);
            foreach (Vector2Int n in GetNeighbors(cell))
            {
                if (visited.Contains(n) || !grid.ContainsKey(n) || grid[n].color != color) continue;
                visited.Add(n);
                queue.Enqueue(n);
            }
        }
        return result;
    }

    private List<Vector2Int> GetNeighbors(Vector2Int cell)
    {
        int[][] even = { new[] { -1, 0 }, new[] { 1, 0 }, new[] { 0, -1 }, new[] { -1, -1 }, new[] { 0, 1 }, new[] { -1, 1 } };
        int[][] odd = { new[] { -1, 0 }, new[] { 1, 0 }, new[] { 1, -1 }, new[] { 0, -1 }, new[] { 1, 1 }, new[] { 0, 1 } };
        int[][] dirs = cell.y % 2 == 0 ? even : odd;
        List<Vector2Int> result = new List<Vector2Int>();
        foreach (int[] d in dirs)
        {
            Vector2Int n = new Vector2Int(cell.x + d[0], cell.y + d[1]);
            if (n.y < 0 || n.y >= MaxRows) continue;
            int maxC = n.y % 2 == 1 ? Columns - 1 : Columns;
            if (n.x < 0 || n.x >= maxC) continue;
            result.Add(n);
        }
        return result;
    }

    private void RemoveFloatingBubbles()
    {
        HashSet<Vector2Int> anchored = new HashSet<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        foreach (Vector2Int cell in grid.Keys)
        {
            if (cell.y != 0) continue;
            anchored.Add(cell);
            queue.Enqueue(cell);
        }
        while (queue.Count > 0)
        {
            Vector2Int cell = queue.Dequeue();
            foreach (Vector2Int n in GetNeighbors(cell))
            {
                if (anchored.Contains(n) || !grid.ContainsKey(n)) continue;
                anchored.Add(n);
                queue.Enqueue(n);
            }
        }
        List<Vector2Int> floating = new List<Vector2Int>();
        foreach (Vector2Int cell in grid.Keys)
        {
            if (!anchored.Contains(cell)) floating.Add(cell);
        }
        foreach (Vector2Int cell in floating)
        {
            ShowPop(CellToPosition(cell));
            Destroy(grid[cell].image.gameObject);
            grid.Remove(cell);
        }
        if (floating.Count > 0) AddScore(floating.Count * 15);
    }

    private void ShowPop(Vector2 position)
    {
        if (popSprite == null) return;
        GameObject obj = CreateImage(playRoot, "Pop", popSprite, new Vector2(82, 82), position, Image.Type.Simple);
        Destroy(obj, 0.28f);
    }

    private bool IsGameOver()
    {
        foreach (BubbleNode node in grid.Values)
        {
            if (CellToPosition(node.cell).y <= -260f) return true;
        }
        return false;
    }

    private void AddScore(int value)
    {
        score += value;
        if (score > bestScore)
        {
            bestScore = score;
            PlayerPrefs.SetInt(BestScoreKey, bestScore);
            PlayerPrefs.Save();
        }
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = score.ToString("0000");
        if (bestText != null) bestText.text = bestScore.ToString("0000");
    }

    private void TogglePause()
    {
        if (!isPlaying || isGameOver) return;
        isPaused = !isPaused;
        pauseOverlay.SetActive(isPaused);
    }

    private void ToggleSettings()
    {
        if (settingOverlay == null) return;
        settingOverlay.SetActive(!settingOverlay.activeSelf);
    }

    private void ToggleMusic()
    {
        musicEnabled = !musicEnabled;
        if (musicSource != null) musicSource.mute = !musicEnabled;
        if (musicStatusText != null) musicStatusText.text = musicEnabled ? "Nhạc chờ: BẬT" : "Nhạc chờ: TẮT";
    }

    private void EndGame()
    {
        isGameOver = true;
        isPlaying = false;
        Play(gameOverClip);
        if (finalScoreText != null) finalScoreText.text = "SCORE: " + score.ToString("0000") + "\nBEST: " + bestScore.ToString("0000");
        ShowOnly(gameOverRoot);
    }

    private bool CanControl()
    {
        return isPlaying && !isPaused && !isGameOver;
    }

    private void HandleKeyboardInput()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;
        if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame) AdjustAim(-8f);
        if (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame) AdjustAim(8f);
        if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame) ShootBubble();
        if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame) SwapBubble();
        if (keyboard.escapeKey.wasPressedThisFrame || keyboard.pKey.wasPressedThisFrame) TogglePause();
#else
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) AdjustAim(-8f);
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) AdjustAim(8f);
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.Space)) ShootBubble();
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) SwapBubble();
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)) TogglePause();
#endif
    }

    private void ClearAllBubbles()
    {
        foreach (BubbleNode node in grid.Values)
        {
            if (node.image != null) Destroy(node.image.gameObject);
        }
        grid.Clear();
        foreach (Image image in temporaryImages)
        {
            if (image != null) Destroy(image.gameObject);
        }
        temporaryImages.Clear();
        if (currentBubbleImage != null) Destroy(currentBubbleImage.gameObject);
    }

    private void Play(AudioClip clip)
    {
        if (clip != null && sfxSource != null) sfxSource.PlayOneShot(clip);
    }
}
