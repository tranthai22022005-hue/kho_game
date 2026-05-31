using UnityEngine;

public class PooledObject : MonoBehaviour
{
    public ObjectPool OriginPool { get; private set; }

    public void SetOriginPool(ObjectPool pool)
    {
        OriginPool = pool;
    }

    public void ReturnToPool()
    {
        if (OriginPool != null)
        {
            OriginPool.Return(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}