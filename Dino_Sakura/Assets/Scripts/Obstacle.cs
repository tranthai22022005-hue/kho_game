using UnityEngine;

[RequireComponent(typeof(PooledObject))]
public class Obstacle : MonoBehaviour
{
    [SerializeField] private float destroyX = -12f;

    private PooledObject pooledObject;

    private void Awake()
    {
        pooledObject = GetComponent<PooledObject>();
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.CurrentState != GameState.Playing) return;

        transform.position += Vector3.left * GameManager.Instance.WorldSpeed * Time.deltaTime;

        if (transform.position.x <= destroyX)
        {
            pooledObject.ReturnToPool();
        }
    }
}