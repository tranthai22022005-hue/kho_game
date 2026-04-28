using UnityEngine;
using TMPro; // BẮT BUỘC phải có dòng này để dùng TextMeshPro

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int score = 0;
    // Đổi 'Text' thành 'TextMeshProUGUI' để khớp với object trong Unity của bạn
    public TextMeshProUGUI scoreText;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void AddScore()
    {
        score++;
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
        else
        {
            Debug.LogError("LỖI: Bạn chưa kéo thả object ScoreText vào GameManager!");
        }
    }

    public void GameOver()
    {
        Time.timeScale = 0;
        Debug.Log("Game Over");
    }
}