
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

public class MemoryMatchGame : MonoBehaviour
{
    private const string BestScoreKey = "MEMORY_MATCH_BLUE_CRYSTAL_BEST";
    private Canvas canvas;
    private Font font;

    private GameObject menuRoot, levelRoot, gameRoot, pauseRoot, settingRoot, gameOverRoot;
    private RectTransform cardGridRoot;
    private Text scoreValueText, bestValueText, movesValueText, timeValueText, finalText, settingMusicText;
    private AudioSource sfxSource, musicSource;

    private Sprite bgMenu, bgGame, titleLogo, menuPanel, levelPanel, settingsRound, infoRound, musicOn, musicOff;
    private Sprite scorePanel, bestPanel, gameOverPanel, youWinPanel, retryButton, menuButton, pauseButton;
    private Sprite cardBack;
    private readonly List<Sprite> faceSprites = new List<Sprite>();
    private readonly List<CardView> cards = new List<CardView>();
    private CardView firstCard, secondCard;

    private AudioClip clickClip, flipClip, matchClip, mismatchClip, winClip;
    private bool busy, playing, paused, musicEnabled = true;
    private int currentLevel = 1;
    private int rows = 4, cols = 4;
    private int score, best, moves, matchedPairs, totalPairs;
    private float elapsed;
    private System.Random rng = new System.Random();

    private class CardView
    {
        public int id;
        public bool opened;
        public bool matched;
        public Button button;
        public Image image;
    }

    private void Awake()
    {
        Application.targetFrameRate = 60;
        LoadAssets();
        CreateCanvas();
        CreateEventSystem();
        CreateAudio();
        BuildMenu();
        BuildLevelSelect();
        BuildGame();
        BuildPause();
        BuildSettings();
        BuildGameOver();
        ShowMenu();
    }

    private void Update()
    {
        if (playing && !paused)
        {
            elapsed += Time.deltaTime;
            UpdateTimeUI();
        }

#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            if (playing) TogglePause(); else ShowMenu();
        }
#else
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (playing) TogglePause(); else ShowMenu();
        }
#endif
    }

    private void LoadAssets()
    {
        bgMenu = LoadSprite("BlueCrystal/Backgrounds/menu_background_day");
        bgGame = LoadSprite("BlueCrystal/Backgrounds/game_background_ui_space");
        titleLogo = LoadSprite("BlueCrystal/Menu/Modules/title_logo");
        menuPanel = LoadSprite("BlueCrystal/Menu/Modules/menu_panel");
        levelPanel = LoadSprite("BlueCrystal/Menu/Modules/level_select_panel");
        settingsRound = LoadSprite("BlueCrystal/Menu/Modules/settings_round");
        infoRound = LoadSprite("BlueCrystal/Menu/Modules/info_round");
        musicOn = LoadSprite("BlueCrystal/Menu/Modules/music_on_round");
        musicOff = LoadSprite("BlueCrystal/Menu/Modules/music_off_round");
        scorePanel = LoadSprite("BlueCrystal/Game/Modules/score_panel");
        bestPanel = LoadSprite("BlueCrystal/Game/Modules/best_panel");
        gameOverPanel = LoadSprite("BlueCrystal/Game/Modules/game_over_panel");
        youWinPanel = LoadSprite("BlueCrystal/Game/Modules/you_win_panel");
        retryButton = LoadSprite("BlueCrystal/Game/Modules/retry_button");
        menuButton = LoadSprite("BlueCrystal/Game/Modules/menu_button");
        pauseButton = LoadSprite("BlueCrystal/Menu/Buttons/pause");
        cardBack = LoadSprite("BlueCrystal/Cards/Back/card_back");
        for (int i = 1; i <= 16; i++)
        {
            string[] names = {"swan","lotus","moon_orb","fish","bellflower","butterfly","pearl_shell","key","castle","crystal","crown","compass_orb","feather","treasure_chest","snowflake","harp"};
            faceSprites.Add(LoadSprite("BlueCrystal/Cards/Faces/card_" + i.ToString("00") + "_" + names[i-1]));
        }
        clickClip = Resources.Load<AudioClip>("Audio/button_click");
        flipClip = Resources.Load<AudioClip>("Audio/card_flip");
        matchClip = Resources.Load<AudioClip>("Audio/card_match");
        mismatchClip = Resources.Load<AudioClip>("Audio/card_mismatch");
        winClip = Resources.Load<AudioClip>("Audio/game_win");
    }

    private Sprite LoadSprite(string path)
    {
        Texture2D tex = Resources.Load<Texture2D>(path);
        if (tex == null)
        {
            Debug.LogWarning("Missing texture: " + path);
            return null;
        }
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
    }

    private void CreateCanvas()
    {
        GameObject go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
    }

    private void CreateEventSystem()
    {
        EventSystem es = FindObjectOfType<EventSystem>();
        GameObject obj;
        if (es == null) obj = new GameObject("EventSystem", typeof(EventSystem));
        else obj = es.gameObject;
        foreach (BaseInputModule m in obj.GetComponents<BaseInputModule>()) DestroyImmediate(m);
#if ENABLE_INPUT_SYSTEM
        obj.AddComponent<InputSystemUIInputModule>();
#else
        obj.AddComponent<StandaloneInputModule>();
#endif
    }

    private void CreateAudio()
    {
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.volume = 0.85f;
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = 0.25f;
        musicSource.clip = winClip; // dùng âm nền nhẹ nếu bộ ảnh không có nhạc riêng
        if (musicSource.clip != null) musicSource.Play();
    }

    private void BuildMenu()
    {
        menuRoot = FullRoot("Menu", bgMenu);
        Image(titleLogo, menuRoot.transform, "Title", new Vector2(860, 345), new Vector2(0, 510), false, true);
        GameObject panel = Image(menuPanel, menuRoot.transform, "MenuPanel", new Vector2(560, 750), new Vector2(0, -170), false, true);
        Hotspot(panel.transform, "PlayHotspot", new Vector2(390, 115), new Vector2(0, 135), () => StartLevel(1));
        Hotspot(panel.transform, "LevelsHotspot", new Vector2(390, 115), new Vector2(0, -70), ShowLevels);
        Hotspot(panel.transform, "SettingsHotspot", new Vector2(390, 115), new Vector2(0, -270), ShowSettings);
        ImageButton(musicOn, menuRoot.transform, "Music", new Vector2(108, 126), new Vector2(410, 725), ToggleMusic, true);
        ImageButton(infoRound, menuRoot.transform, "Info", new Vector2(108, 126), new Vector2(-410, 725), ShowSettings, true);
        TextLabel(menuRoot.transform, "MEMORY MATCH 2D", 0, -820, 32, new Vector2(820, 70), Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
    }

    private void BuildLevelSelect()
    {
        levelRoot = FullRoot("LevelSelect", bgMenu);
        GameObject panel = Image(levelPanel, levelRoot.transform, "LevelPanel", new Vector2(700, 900), new Vector2(0, 20), false, true);
        TextLabel(panel.transform, "CHỌN MÀN", 0, 300, 58, new Vector2(520, 80), Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
        for (int i = 1; i <= 4; i++)
        {
            int lv = i;
            TextButton(panel.transform, "LEVEL " + i, 0, 200 - (i - 1) * 135, 430, 95, () => StartLevel(lv), 40);
        }
        TextButton(panel.transform, "VỀ MENU", 0, -375, 430, 90, ShowMenu, 36);
    }

    private void BuildGame()
    {
        gameRoot = FullRoot("Game", bgGame);
        GameObject scoreObj = Image(scorePanel, gameRoot.transform, "ScorePanel", new Vector2(335, 108), new Vector2(-310, 805), false, true);
        TextLabel(scoreObj.transform, "SCORE", 0, 26, 29, new Vector2(270, 42), Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
        scoreValueText = TextLabel(scoreObj.transform, "0000", 0, -22, 34, new Vector2(270, 48), Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
        GameObject bestObj = Image(bestPanel, gameRoot.transform, "BestPanel", new Vector2(335, 108), new Vector2(120, 805), false, true);
        TextLabel(bestObj.transform, "BEST", 0, 26, 29, new Vector2(270, 42), Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
        bestValueText = TextLabel(bestObj.transform, "0000", 0, -22, 34, new Vector2(270, 48), Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
        ImageButton(pauseButton, gameRoot.transform, "Pause", new Vector2(125, 78), new Vector2(440, 805), TogglePause, true);
        GameObject movesObj = Image(scorePanel, gameRoot.transform, "MovesPanel", new Vector2(245, 78), new Vector2(-345, 690), false, true);
        movesValueText = TextLabel(movesObj.transform, "MOVES: 0", 0, 0, 27, new Vector2(220, 52), Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
        GameObject timeObj = Image(bestPanel, gameRoot.transform, "TimePanel", new Vector2(245, 78), new Vector2(345, 690), false, true);
        timeValueText = TextLabel(timeObj.transform, "TIME: 00:00", 0, 0, 27, new Vector2(220, 52), Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
        cardGridRoot = Rect(gameRoot.transform, "CardGrid", new Vector2(880, 1040), new Vector2(0, -50));
    }

    private void BuildPause()
    {
        pauseRoot = FullRoot("Pause", null);
        Overlay(pauseRoot.transform);
        GameObject panel = Image(levelPanel, pauseRoot.transform, "PausePanel", new Vector2(680, 650), Vector2.zero, false, true);
        TextLabel(panel.transform, "TẠM DỪNG", 0, 190, 58, new Vector2(520, 90), Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
        TextButton(panel.transform, "TIẾP TỤC", 0, 60, 420, 90, TogglePause, 38);
        TextButton(panel.transform, "CHƠI LẠI", 0, -60, 420, 90, () => StartLevel(currentLevel), 38);
        TextButton(panel.transform, "VỀ MENU", 0, -185, 420, 90, ShowMenu, 38);
    }

    private void BuildSettings()
    {
        settingRoot = FullRoot("Settings", null);
        Overlay(settingRoot.transform);
        GameObject panel = Image(levelPanel, settingRoot.transform, "SettingsPanel", new Vector2(720, 760), Vector2.zero, false, true);
        TextLabel(panel.transform, "CÀI ĐẶT", 0, 245, 58, new Vector2(560, 80), Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
        settingMusicText = TextLabel(panel.transform, "Nhạc chờ: BẬT", 0, 135, 36, new Vector2(560, 70), Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
        TextLabel(panel.transform, "Game lật thẻ 2D\nChọn hai thẻ giống nhau để ghi điểm.\nHoàn thành toàn bộ cặp thẻ để chiến thắng.", 0, 0, 28, new Vector2(600, 190), Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
        TextButton(panel.transform, "BẬT / TẮT NHẠC", 0, -160, 430, 85, ToggleMusic, 31);
        TextButton(panel.transform, "ĐÓNG", 0, -275, 430, 85, ClosePopup, 34);
    }

    private void BuildGameOver()
    {
        gameOverRoot = FullRoot("GameOver", bgGame);
        GameObject panel = Image(youWinPanel, gameOverRoot.transform, "WinPanel", new Vector2(780, 655), new Vector2(0, 130), false, true);
        finalText = TextLabel(panel.transform, "HOÀN THÀNH!\nSCORE: 0000\nBEST: 0000", 0, 25, 44, new Vector2(620, 240), Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
        ImageButton(retryButton, gameOverRoot.transform, "Retry", new Vector2(285, 165), new Vector2(-170, -300), () => StartLevel(currentLevel), true);
        ImageButton(menuButton, gameOverRoot.transform, "Menu", new Vector2(285, 165), new Vector2(170, -300), ShowMenu, true);
    }

    private void StartLevel(int level)
    {
        currentLevel = Mathf.Clamp(level, 1, 4);
        if (currentLevel == 1) { cols = 4; rows = 4; }
        if (currentLevel == 2) { cols = 4; rows = 5; }
        if (currentLevel == 3) { cols = 4; rows = 6; }
        if (currentLevel == 4) { cols = 4; rows = 7; }
        StartGame();
    }

    private void StartGame()
    {
        foreach (CardView c in cards) if (c != null && c.button != null) Destroy(c.button.gameObject);
        cards.Clear(); firstCard = null; secondCard = null; busy = false;
        score = 0; moves = 0; elapsed = 0; matchedPairs = 0; totalPairs = rows * cols / 2;
        best = PlayerPrefs.GetInt(BestScoreKey, 0);
        playing = true; paused = false;
        UpdateScoreUI(); UpdateMovesUI(); UpdateTimeUI();
        ShowOnly(gameRoot);
        BuildCards();
    }

    private void BuildCards()
    {
        List<int> ids = new List<int>();
        for (int i = 0; i < totalPairs; i++) { ids.Add(i % faceSprites.Count); ids.Add(i % faceSprites.Count); }
        for (int i = ids.Count - 1; i > 0; i--) { int j = rng.Next(i + 1); int tmp = ids[i]; ids[i] = ids[j]; ids[j] = tmp; }
        float maxW = 830f, maxH = 1120f;
        float gap = rows >= 7 ? 10f : 16f;
        float cardW = (maxW - gap * (cols - 1)) / cols;
        float cardH = (maxH - gap * (rows - 1)) / rows;
        float ratio = 0.78f;
        if (cardW / cardH > ratio) cardW = cardH * ratio; else cardH = cardW / ratio;
        float totalW = cols * cardW + (cols - 1) * gap;
        float totalH = rows * cardH + (rows - 1) * gap;
        float startX = -totalW / 2f + cardW / 2f;
        float startY = totalH / 2f - cardH / 2f;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                int idx = r * cols + c;
                CardView cv = new CardView(); cv.id = ids[idx];
                GameObject obj = Image(cardBack, cardGridRoot, "Card", new Vector2(cardW, cardH), new Vector2(startX + c * (cardW + gap), startY - r * (cardH + gap)), true, true);
                cv.image = obj.GetComponent<Image>();
                cv.button = obj.AddComponent<Button>();
                cv.button.targetGraphic = cv.image;
                cv.button.onClick.AddListener(() => OnCardClicked(cv));
                cards.Add(cv);
            }
        }
    }

    private void OnCardClicked(CardView card)
    {
        if (!playing || paused || busy || card.opened || card.matched) return;
        Play(flipClip);
        OpenCard(card);
        if (firstCard == null) { firstCard = card; return; }
        secondCard = card;
        moves++;
        UpdateMovesUI();
        StartCoroutine(CheckPair());
    }

    private void OpenCard(CardView c)
    {
        c.opened = true;
        c.image.sprite = faceSprites[c.id];
    }

    private void CloseCard(CardView c)
    {
        c.opened = false;
        c.image.sprite = cardBack;
    }

    private IEnumerator CheckPair()
    {
        busy = true;
        yield return new WaitForSeconds(0.45f);
        if (firstCard.id == secondCard.id)
        {
            firstCard.matched = true; secondCard.matched = true;
            firstCard.button.interactable = false; secondCard.button.interactable = false;
            matchedPairs++;
            score += 100 + Mathf.Max(0, 30 - moves);
            Play(matchClip);
            UpdateScoreUI();
            if (matchedPairs >= totalPairs) WinGame();
        }
        else
        {
            CloseCard(firstCard); CloseCard(secondCard);
            score = Mathf.Max(0, score - 5);
            Play(mismatchClip);
            UpdateScoreUI();
        }
        firstCard = null; secondCard = null; busy = false;
    }

    private void WinGame()
    {
        playing = false;
        int timeBonus = Mathf.Max(0, 300 - Mathf.FloorToInt(elapsed));
        score += timeBonus;
        if (score > best)
        {
            best = score;
            PlayerPrefs.SetInt(BestScoreKey, best);
            PlayerPrefs.Save();
        }
        Play(winClip);
        finalText.text = "HOÀN THÀNH!\nSCORE: " + score.ToString("0000") + "\nBEST: " + best.ToString("0000") + "\nMOVES: " + moves + "   TIME: " + FormatTime(elapsed);
        ShowOnly(gameOverRoot);
    }

    private void TogglePause()
    {
        if (!playing) return;
        paused = !paused;
        pauseRoot.SetActive(paused);
    }

    private void ShowSettings()
    {
        settingRoot.SetActive(true);
    }

    private void ClosePopup()
    {
        settingRoot.SetActive(false);
        levelRoot.SetActive(false);
        if (!paused) pauseRoot.SetActive(false);
    }

    private void ShowLevels()
    {
        ShowOnly(levelRoot);
    }

    private void ShowMenu()
    {
        playing = false; paused = false;
        ShowOnly(menuRoot);
    }

    private void ShowOnly(GameObject root)
    {
        menuRoot.SetActive(root == menuRoot);
        levelRoot.SetActive(root == levelRoot);
        gameRoot.SetActive(root == gameRoot);
        gameOverRoot.SetActive(root == gameOverRoot);
        pauseRoot.SetActive(false);
        settingRoot.SetActive(false);
    }

    private void ToggleMusic()
    {
        musicEnabled = !musicEnabled;
        if (musicSource != null) musicSource.mute = !musicEnabled;
        if (settingMusicText != null) settingMusicText.text = musicEnabled ? "Nhạc chờ: BẬT" : "Nhạc chờ: TẮT";
    }

    private void UpdateScoreUI()
    {
        if (scoreValueText != null) scoreValueText.text = score.ToString("0000");
        if (score > best)
        {
            best = score; PlayerPrefs.SetInt(BestScoreKey, best); PlayerPrefs.Save();
        }
        if (bestValueText != null) bestValueText.text = best.ToString("0000");
    }

    private void UpdateMovesUI()
    {
        if (movesValueText != null) movesValueText.text = "MOVES: " + moves;
    }

    private void UpdateTimeUI()
    {
        if (timeValueText != null) timeValueText.text = "TIME: " + FormatTime(elapsed);
    }

    private string FormatTime(float t)
    {
        int s = Mathf.FloorToInt(t); return (s / 60).ToString("00") + ":" + (s % 60).ToString("00");
    }

    private GameObject FullRoot(string name, Sprite background)
    {
        GameObject root = new GameObject(name, typeof(RectTransform));
        root.transform.SetParent(canvas.transform, false);
        RectTransform rt = root.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        if (background != null)
        {
            Image img = root.AddComponent<Image>(); img.sprite = background; img.preserveAspect = false; img.raycastTarget = false;
        }
        return root;
    }

    private void Overlay(Transform parent)
    {
        GameObject o = new GameObject("Dim", typeof(RectTransform), typeof(Image));
        o.transform.SetParent(parent, false);
        RectTransform rt = o.GetComponent<RectTransform>(); rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        Image img = o.GetComponent<Image>(); img.color = new Color(0,0,0,0.52f); img.raycastTarget = true;
    }

    private GameObject Image(Sprite sprite, Transform parent, string name, Vector2 size, Vector2 pos, bool raycast, bool preserve)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image));
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.GetComponent<RectTransform>(); rt.anchorMin = rt.anchorMax = new Vector2(0.5f,0.5f); rt.pivot = new Vector2(0.5f,0.5f); rt.sizeDelta = size; rt.anchoredPosition = pos;
        Image img = obj.GetComponent<Image>(); img.sprite = sprite; img.type = Image.Type.Simple; img.preserveAspect = preserve; img.raycastTarget = raycast; img.color = Color.white;
        return obj;
    }

    private Button ImageButton(Sprite sprite, Transform parent, string name, Vector2 size, Vector2 pos, UnityEngine.Events.UnityAction action, bool preserve)
    {
        GameObject obj = Image(sprite, parent, name, size, pos, true, preserve);
        Button b = obj.AddComponent<Button>(); b.targetGraphic = obj.GetComponent<Image>(); b.onClick.AddListener(() => { Play(clickClip); action?.Invoke(); });
        return b;
    }

    private Button TextButton(Transform parent, string label, float x, float y, float w, float h, UnityEngine.Events.UnityAction action, int size)
    {
        GameObject obj = Image(scorePanel, parent, label + "Button", new Vector2(w,h), new Vector2(x,y), true, true);
        Button b = obj.AddComponent<Button>(); b.targetGraphic = obj.GetComponent<Image>(); b.onClick.AddListener(() => { Play(clickClip); action?.Invoke(); });
        TextLabel(obj.transform, label, 0, 0, size, new Vector2(w*0.85f, h*0.72f), Color.white, FontStyle.Bold, TextAnchor.MiddleCenter);
        return b;
    }

    private void Hotspot(Transform parent, string name, Vector2 size, Vector2 pos, UnityEngine.Events.UnityAction action)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.GetComponent<RectTransform>(); rt.anchorMin = rt.anchorMax = new Vector2(0.5f,0.5f); rt.pivot = new Vector2(0.5f,0.5f); rt.sizeDelta = size; rt.anchoredPosition = pos;
        Image img = obj.GetComponent<Image>(); img.color = new Color(1,1,1,0); img.raycastTarget = true;
        Button b = obj.GetComponent<Button>(); b.targetGraphic = img; b.onClick.AddListener(() => { Play(clickClip); action?.Invoke(); });
    }

    private RectTransform Rect(Transform parent, string name, Vector2 size, Vector2 pos)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform)); obj.transform.SetParent(parent, false);
        RectTransform rt = obj.GetComponent<RectTransform>(); rt.anchorMin = rt.anchorMax = new Vector2(0.5f,0.5f); rt.pivot = new Vector2(0.5f,0.5f); rt.sizeDelta = size; rt.anchoredPosition = pos; return rt;
    }

    private Text TextLabel(Transform parent, string text, float x, float y, int size, Vector2 rect, Color color, FontStyle style, TextAnchor anchor)
    {
        GameObject obj = new GameObject("Text", typeof(RectTransform), typeof(Text), typeof(Outline)); obj.transform.SetParent(parent, false);
        RectTransform rt = obj.GetComponent<RectTransform>(); rt.anchorMin = rt.anchorMax = new Vector2(0.5f,0.5f); rt.pivot = new Vector2(0.5f,0.5f); rt.sizeDelta = rect; rt.anchoredPosition = new Vector2(x,y);
        Text t = obj.GetComponent<Text>(); t.text = text; t.font = font; t.fontSize = size; t.alignment = anchor; t.color = color; t.fontStyle = style; t.resizeTextForBestFit = true; t.resizeTextMinSize = 16; t.resizeTextMaxSize = size; t.raycastTarget = false;
        Outline o = obj.GetComponent<Outline>(); o.effectColor = new Color(0.02f,0.09f,0.28f,0.95f); o.effectDistance = new Vector2(2,-2);
        return t;
    }

    private void Play(AudioClip clip)
    {
        if (clip != null && sfxSource != null) sfxSource.PlayOneShot(clip);
    }
}
