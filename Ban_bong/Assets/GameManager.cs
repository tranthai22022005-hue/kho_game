using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public int score = 0;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Giữ điểm số khi chuyển cảnh
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddScore(int points)
    {
        score += points;
        // Bạn có thể update UI Text tại đây
        Debug.Log($"[SCORE UPDATED] Total: {score}");
    }
}