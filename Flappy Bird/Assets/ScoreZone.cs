using UnityEngine;

// Khi chim đi qua → + điểm
public class ScoreZone : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            FindObjectOfType<GameManager>().AddScore();
        }
    }
}

