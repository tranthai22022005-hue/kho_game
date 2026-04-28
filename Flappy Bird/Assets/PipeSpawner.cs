using UnityEngine;

public class PipeSpawner : MonoBehaviour
{
    public GameObject pipePrefab;
    public float spawnTime = 2f;
    public float minY = -1.5f;
    public float maxY = 2.5f;
    public float spawnX = 10f;

    private float timer;

    void Start()
    {
        if (pipePrefab == null)
        {
            Debug.LogError("pipePrefab chưa được gán trong Inspector. Hãy kéo prefab cột vào ô Pipe Prefab của object PipeSpawner.");
        }
    }

    void Update()
    {
        if (pipePrefab == null) return;

        timer += Time.deltaTime;

        if (timer >= spawnTime)
        {
            timer = 0f;

            float randomY = Random.Range(minY, maxY);
            Instantiate(pipePrefab, new Vector3(spawnX, randomY, 0f), Quaternion.identity);
        }
    }
}