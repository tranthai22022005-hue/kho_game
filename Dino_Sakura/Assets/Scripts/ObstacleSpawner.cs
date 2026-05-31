using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnEntry
    {
        public string poolId;
        public float weight = 1f;
        public float yPosition = -2.65f;
        public float minScoreToSpawn = 0f;
    }

    [Header("References")]
    [SerializeField] private ObjectPool objectPool;

    [Header("Spawn Position")]
    [SerializeField] private float spawnX = 11f;

    [Header("Difficulty")]
    [SerializeField] private float startMinDelay = 1.35f;
    [SerializeField] private float startMaxDelay = 2.2f;
    [SerializeField] private float hardMinDelay = 0.65f;
    [SerializeField] private float hardMaxDelay = 1.15f;
    [SerializeField] private float scoreToReachHardest = 2500f;

    [Header("Obstacle Entries")]
    [SerializeField] private SpawnEntry[] entries;

    private float timer;
    private float nextDelay;
    private int lastEntryIndex = -1;

    private void Start()
    {
        ScheduleNextSpawn();
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.CurrentState != GameState.Playing) return;

        timer += Time.deltaTime;
        if (timer >= nextDelay)
        {
            Spawn();
            ScheduleNextSpawn();
        }
    }

    private void Spawn()
    {
        if (objectPool == null || entries == null || entries.Length == 0) return;

        int index = PickEntryIndex();
        if (index < 0) return;

        SpawnEntry entry = entries[index];
        Vector3 position = new Vector3(spawnX, entry.yPosition, 0f);
        objectPool.Get(entry.poolId, position, Quaternion.identity);
        lastEntryIndex = index;
    }

    private int PickEntryIndex()
    {
        float score = GameManager.Instance != null ? GameManager.Instance.Score : 0f;
        float totalWeight = 0f;

        for (int i = 0; i < entries.Length; i++)
        {
            if (entries[i] == null) continue;
            if (score < entries[i].minScoreToSpawn) continue;

            float weight = Mathf.Max(0f, entries[i].weight);

            // Thuật toán spawn có kiểm soát: giảm xác suất lặp lại cùng obstacle liên tiếp.
            if (i == lastEntryIndex)
            {
                weight *= 0.25f;
            }

            totalWeight += weight;
        }

        if (totalWeight <= 0f) return -1;

        float randomPoint = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        for (int i = 0; i < entries.Length; i++)
        {
            if (entries[i] == null) continue;
            if (score < entries[i].minScoreToSpawn) continue;

            float weight = Mathf.Max(0f, entries[i].weight);
            if (i == lastEntryIndex)
            {
                weight *= 0.25f;
            }

            cumulative += weight;
            if (randomPoint <= cumulative)
            {
                return i;
            }
        }

        return entries.Length - 1;
    }

    private void ScheduleNextSpawn()
    {
        timer = 0f;

        float score = GameManager.Instance != null ? GameManager.Instance.Score : 0f;
        float t = Mathf.Clamp01(score / scoreToReachHardest);

        float minDelay = Mathf.Lerp(startMinDelay, hardMinDelay, t);
        float maxDelay = Mathf.Lerp(startMaxDelay, hardMaxDelay, t);

        nextDelay = Random.Range(minDelay, maxDelay);
    }
}