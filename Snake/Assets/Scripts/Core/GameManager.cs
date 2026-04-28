using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int score = 0;
    public int winScore = 10; // Đạt 10 điểm là thắng
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI resultText;

    public GameObject gameOverPanel;
    public GameObject winPanel; // Thêm ô này để kéo WinPanel vào

    private bool isGameOver = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        Time.timeScale = 1;
        isGameOver = false;
        score = 0;

        UpdateUI();

        // Ẩn tất cả các bảng khi bắt đầu
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
    }

    public void AddScore(int amount)
    {
        if (isGameOver) return;
        score += amount;
        UpdateUI();

        // Kiểm tra điều kiện thắng
        if (score >= winScore)
        {
            Win();
        }
    }

    void UpdateUI()
    {
        if (scoreText != null) scoreText.text = "Score: " + score;
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0;
    }

    public void Win()
    {
        if (isGameOver) return;
        isGameOver = true;

        // Hiện bảng chiến thắng
        if (winPanel != null) winPanel.SetActive(true);

        Debug.Log("Chúc mừng! Bạn đã thắng.");
        Time.timeScale = 0; // Dừng game lại
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}