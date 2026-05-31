using UnityEngine;

public class GroundScroller : MonoBehaviour
{
    [Header("Ground Tiles")]
    [SerializeField] private Transform[] groundTiles;

    [Header("Scroll Settings")]
    [SerializeField] private float tileWidth = 4f;
    [SerializeField] private float resetX = -6f;

    [Header("Options")]
    [SerializeField] private bool moveWhenDead = false;
    [SerializeField] private bool autoSortTilesOnStart = true;

    private void Start()
    {
        if (autoSortTilesOnStart)
        {
            SortTilesByX();
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;

        GameState state = GameManager.Instance.CurrentState;

        if (state == GameState.Idle) return;

        if (state == GameState.Dead && !moveWhenDead) return;

        if (groundTiles == null || groundTiles.Length == 0) return;

        float speed = GameManager.Instance.WorldSpeed;

        for (int i = 0; i < groundTiles.Length; i++)
        {
            Transform tile = groundTiles[i];

            if (tile == null) continue;

            MoveTile(tile, speed);

            if (tile.position.x <= resetX)
            {
                RecycleTile(tile);
            }
        }
    }

    private void MoveTile(Transform tile, float speed)
    {
        tile.position += Vector3.left * speed * Time.deltaTime;
    }

    private void RecycleTile(Transform tile)
    {
        float rightMostX = GetRightMostX();

        tile.position = new Vector3(
            rightMostX + tileWidth,
            tile.position.y,
            tile.position.z
        );
    }

    private float GetRightMostX()
    {
        float rightMostX = float.MinValue;

        for (int i = 0; i < groundTiles.Length; i++)
        {
            Transform tile = groundTiles[i];

            if (tile == null) continue;

            if (tile.position.x > rightMostX)
            {
                rightMostX = tile.position.x;
            }
        }

        return rightMostX;
    }

    private void SortTilesByX()
    {
        if (groundTiles == null || groundTiles.Length <= 1) return;

        for (int i = 0; i < groundTiles.Length - 1; i++)
        {
            for (int j = i + 1; j < groundTiles.Length; j++)
            {
                if (groundTiles[i] == null || groundTiles[j] == null) continue;

                if (groundTiles[i].position.x > groundTiles[j].position.x)
                {
                    Transform temp = groundTiles[i];
                    groundTiles[i] = groundTiles[j];
                    groundTiles[j] = temp;
                }
            }
        }
    }

    private void OnValidate()
    {
        if (tileWidth <= 0f)
        {
            tileWidth = 1f;
        }
    }
}