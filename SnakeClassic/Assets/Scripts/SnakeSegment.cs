using UnityEngine;

public class SnakeSegment : MonoBehaviour
{
    public Vector2Int GridPosition { get; private set; }

    public void SetGridPosition(Vector2Int position, float cellSize, Vector2 boardOffset)
    {
        GridPosition = position;

        transform.position = new Vector3(
            boardOffset.x + position.x * cellSize,
            boardOffset.y + position.y * cellSize,
            0f
        );
    }
}