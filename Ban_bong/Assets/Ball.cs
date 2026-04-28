using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    [Header("=== COLOR ===")]
    public string colorID;
    public float checkRadius = 0.7f;

    public bool isShot = false;
    private bool isSettled = false;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isShot || isSettled) return;

        // Va chạm với Top hoặc Ball khác
        if (collision.gameObject.CompareTag("Top") || collision.gameObject.CompareTag("Ball"))
        {
            SettleBall();
        }
    }

    private void SettleBall()
    {
        isSettled = true;
        isShot = false;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        gameObject.tag = "Ball";

        // ✅ Yêu cầu GridManager căn chỉnh vị trí cho thẳng hàng
        if (GridManager.instance != null)
        {
            transform.position = GridManager.instance.GetNearestGridPos(transform.position);
        }

        CheckAndPop();
    }

    private void CheckAndPop()
    {
        List<GameObject> matches = new List<GameObject>();
        FindMatches(gameObject, matches);

        if (matches.Count >= 3)
        {
            // GameManager.instance?.AddScore(matches.Count * 10);
            foreach (var ball in matches)
            {
                Destroy(ball);
            }
        }
    }

    private void FindMatches(GameObject current, List<GameObject> matches)
    {
        matches.Add(current);
        Collider2D[] hits = Physics2D.OverlapCircleAll(current.transform.position, checkRadius);

        foreach (var hit in hits)
        {
            Ball other = hit.GetComponent<Ball>();
            // Chỉ tìm những quả cùng màu, đã đứng yên, và chưa có trong danh sách matches
            if (other != null && other.isSettled && other.colorID == colorID)
            {
                if (!matches.Contains(hit.gameObject))
                {
                    FindMatches(hit.gameObject, matches);
                }
            }
        }
    }
}