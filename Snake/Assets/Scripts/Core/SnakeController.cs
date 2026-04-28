using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeController : MonoBehaviour
{
    public FoodManager foodManager;
    public float moveDelay = 0.2f;
    public GameObject bodyPrefab;

    private Vector2 direction = Vector2.right;
    private List<Transform> body = new List<Transform>();

    void Start()
    {
        InvokeRepeating(nameof(Move), moveDelay, moveDelay);
    }

    void Move()
    {
        Vector3 prevPos = transform.position;

        // di chuyển đầu
        transform.position += (Vector3)direction;

        // thân đi theo
        for (int i = 0; i < body.Count; i++)
        {
            Vector3 temp = body[i].position;
            body[i].position = prevPos;
            prevPos = temp;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) && direction != Vector2.down)
            direction = Vector2.up;
        if (Input.GetKeyDown(KeyCode.DownArrow) && direction != Vector2.up)
            direction = Vector2.down;
        if (Input.GetKeyDown(KeyCode.LeftArrow) && direction != Vector2.right)
            direction = Vector2.left;
        if (Input.GetKeyDown(KeyCode.RightArrow) && direction != Vector2.left)
            direction = Vector2.right;
    }

    public List<Transform> GetBody()
    {
        return body;
    }

    public void Grow()
    {
        // Fix: Sinh ra đốt thân tại vị trí cuối cùng để tránh bị lỗi vị trí (0,0,0)
        Vector3 spawnPos = body.Count > 0 ? body[body.Count - 1].position : transform.position;
        GameObject newPart = Instantiate(bodyPrefab, spawnPos, Quaternion.identity);
        body.Add(newPart.transform);

        // Cộng điểm khi ăn mồi
        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(1);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Food"))
        {
            Destroy(other.gameObject);
            Grow();
            foodManager.SpawnFood();
        }

        // Kiểm tra đúng Tag "Wall" (chữ W viết hoa)
        if (other.CompareTag("Wall"))
        {
            Debug.Log("Va chạm Tường!");
            if (GameManager.Instance != null)
                GameManager.Instance.GameOver();
        }

        if (other.CompareTag("Body"))
        {
            // Fix: Tránh va chạm với đốt thân đầu tiên ngay khi mới bắt đầu
            if (body.Count > 2)
            {
                Debug.Log("Va chạm Thân!");
                if (GameManager.Instance != null)
                    GameManager.Instance.GameOver();
            }
        }
    }
}