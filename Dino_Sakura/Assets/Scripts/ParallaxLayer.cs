using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [SerializeField] private float speedMultiplier = 0.2f;
    [SerializeField] private float tileWidth = 13.5f;
    [SerializeField] private float resetX = -13.5f;
    [SerializeField] private bool moveWhenDead = false;

    private void Update()
    {
        if (GameManager.Instance == null) return;

        GameState state = GameManager.Instance.CurrentState;

        if (state == GameState.Idle) return;
        if (state == GameState.Dead && !moveWhenDead) return;

        float moveSpeed = GameManager.Instance.WorldSpeed * speedMultiplier;
        transform.position += Vector3.left * moveSpeed * Time.deltaTime;

        if (transform.position.x <= resetX)
        {
            transform.position = new Vector3(
                transform.position.x + tileWidth * 2f,
                transform.position.y,
                transform.position.z
            );
        }
    }
}