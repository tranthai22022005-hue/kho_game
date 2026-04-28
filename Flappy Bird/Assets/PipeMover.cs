using UnityEngine;

// Ống di chuyển sang trái
public class PipeMover : MonoBehaviour
{
    public float speed = 2f;

    void Update()
    {
        transform.Translate(Vector2.left * speed * Time.deltaTime);
    }
}

