using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [System.Serializable]
    public class PoolItem
    {
        public string id;
        public GameObject prefab;
        public int preloadCount = 8;
    }

    [Header("Pool Config")]
    [SerializeField] private PoolItem[] items;

    private readonly Dictionary<string, Queue<GameObject>> pools = new Dictionary<string, Queue<GameObject>>();
    private readonly Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();

    private void Awake()
    {
        foreach (PoolItem item in items)
        {
            if (item == null || item.prefab == null || string.IsNullOrWhiteSpace(item.id))
            {
                continue;
            }

            prefabs[item.id] = item.prefab;
            pools[item.id] = new Queue<GameObject>();

            for (int i = 0; i < item.preloadCount; i++)
            {
                GameObject obj = CreateNew(item.id);
                obj.SetActive(false);
                pools[item.id].Enqueue(obj);
            }
        }
    }

    public GameObject Get(string id, Vector3 position, Quaternion rotation)
    {
        if (!prefabs.ContainsKey(id))
        {
            Debug.LogError($"Pool id not found: {id}");
            return null;
        }

        GameObject obj = pools[id].Count > 0 ? pools[id].Dequeue() : CreateNew(id);
        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);
        return obj;
    }

    public void Return(GameObject obj)
    {
        PooledObject pooled = obj.GetComponent<PooledObject>();
        if (pooled == null)
        {
            obj.SetActive(false);
            return;
        }

        string id = obj.name.Replace("(Clone)", "").Trim();
        obj.SetActive(false);

        if (!pools.ContainsKey(id))
        {
            pools[id] = new Queue<GameObject>();
        }

        pools[id].Enqueue(obj);
    }

    public void ReturnAllActiveChildren()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    private GameObject CreateNew(string id)
    {
        GameObject obj = Instantiate(prefabs[id], transform);
        obj.name = id;
        PooledObject pooled = obj.GetComponent<PooledObject>();
        if (pooled == null)
        {
            pooled = obj.AddComponent<PooledObject>();
        }

        pooled.SetOriginPool(this);
        return obj;
    }
}