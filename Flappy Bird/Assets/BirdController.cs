using UnityEngine;

// Điều khiển chim
public class BirdController : MonoBehaviour
{
    public float jumpForce = 5f; // lực bay
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Click chuột → bay lên
        if (Input.GetMouseButtonDown(0))
        {
            rb.linearVelocity = Vector2.up * jumpForce;
        }
    }

    // Va chạm → Game Over
    void OnCollisionEnter2D(Collision2D collision)
    {
        FindObjectOfType<GameManager>().GameOver();
    }
}

