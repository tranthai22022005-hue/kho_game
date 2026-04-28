using UnityEngine;

public class InitialSpawner : MonoBehaviour
{
    [Header("Ball Prefabs")]
    public GameObject Ball_R;
    public GameObject Ball_B;
    public GameObject Ball_Y;

    [Header("Spawn Settings")]
    public int rows = 4;           // Số hàng bóng
    public int columns = 7;        // Số cột bóng
    public float spacingX = 1.05f;
    public float spacingY = 1.05f;
    public float startY = 3.8f;    // Vị trí bắt đầu từ trên xuống

    void Start()
    {
        SpawnBalls();
    }

    private void SpawnBalls()
    {
        GameObject[] ballTypes = { Ball_R, Ball_B, Ball_Y };

        float startX = -(columns - 1) * spacingX / 2f;   // Căn giữa tự động

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                float x = startX + col * spacingX;
                float y = startY - row * spacingY;

                GameObject prefab = ballTypes[Random.Range(0, 3)];
                GameObject ball = Instantiate(prefab, new Vector2(x, y), Quaternion.identity);

                Ball ballScript = ball.GetComponent<Ball>();
                if (ballScript != null)
                {
                    ballScript.colorID = (prefab == Ball_R) ? "Red" :
                                        (prefab == Ball_B) ? "Blue" : "Yellow";
                }
            }
        }
    }

}