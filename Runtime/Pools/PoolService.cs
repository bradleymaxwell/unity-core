using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolService
{
    private const int MaxPoolSize = 100;
    private readonly Dictionary<GameObject, ObjectPool<GameObject>> _pools = new();
    private readonly Logger _logger;

    public PoolService()
    {
        _logger = new Logger(nameof(PoolService));
    }
    
    public void CreatePool<T>(T prefab, int initialSize = 0) where T : MonoBehaviour, IPoolable
    {
        if (PoolExists(prefab.gameObject))
        {
            _logger.LogWarning($"Pool {prefab.name} already exists");
            return;
        }
        
        var pool = new ObjectPool<GameObject>(
            createFunc: () => Object.Instantiate(prefab.gameObject, Vector3.zero, Quaternion.identity),
            actionOnGet: instance => instance.gameObject.SetActive(true),
            actionOnRelease: instance =>
            {
                instance.gameObject.SetActive(false);
                instance.GetComponent<T>().OnReturnToPool();
            },
            actionOnDestroy: Object.Destroy,
            defaultCapacity: initialSize,
            maxSize: MaxPoolSize);
        
        Warm(pool, initialSize);
        _pools.Add(prefab.gameObject, pool);
    }

    public T Get<T>(T prefab) where T : MonoBehaviour, IPoolable
    {
        var pool = GetPool(prefab.gameObject);
        var instance = pool?.Get();
        var component = instance?.GetComponent<T>();
        if (component != null)
        {
            return component;
        }
        
        _logger.LogError($"{instance} retrieved from pool: {prefab.name} does not contain component of type {typeof(T)}");
        return null;
    }

    public void Return(GameObject instance)
    {
        var pool = GetPool(instance.gameObject);
        pool?.Release(instance);
    }

    public void Warm(GameObject prefab, int size)
    {
        var pool = GetPool(prefab);
        Warm(pool, size);
    }

    private static void Warm(IObjectPool<GameObject> pool, int size)
    {
        for (var i = 0; i < size; i++)
        {
            var instance = pool?.Get();
            pool?.Release(instance);
        }
    }

    private IObjectPool<GameObject> GetPool(GameObject prefab)
    {
        if (PoolExists(prefab))
        {
            return _pools[prefab];
        }
        
        _logger.LogError($"Pool for prefab: {prefab.name} does not exist");
        return null;
    }
    
    private bool PoolExists(GameObject prefab)
    {
        return _pools.ContainsKey(prefab);
    }
}
