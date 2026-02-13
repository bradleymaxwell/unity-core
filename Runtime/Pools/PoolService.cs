using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

public class PoolService
{
    private const int MaxPoolSize = 100;
    private readonly Dictionary<GameObject, ObjectPool<GameObject>> _pools = new();
    private readonly Logger _logger;
    private readonly Dictionary<GameObject, GameObject> _poolObjectsByPrefab = new();
    
    public PoolService() : this(new Logger(nameof(PoolService)))
    {
    }

    public PoolService(Logger logger)
    {
        _logger = logger;
    }
    
    public IObjectPool<GameObject> CreatePool<T>(T prefab, int initialSize = 0) where T : Component, IPoolable
    {
        var pool = CreatePool(prefab, onRelease: instance => instance.GetComponent<T>().Reset(), initialSize: initialSize);
        return pool;
    }

    private IObjectPool<GameObject> CreatePool<T>(T prefab, Action<GameObject> onGet = null, Action<GameObject> onRelease = null, int initialSize = 0) where T : Component, IPoolable
    {
        if (PoolExists(prefab.gameObject))
        {
            _logger.LogWarning($"Pool {prefab.name} already exists");
            return _pools[prefab.gameObject];
        }

        var poolObject = GetPoolObject(prefab.gameObject);
        var pool = new ObjectPool<GameObject>(
            createFunc: () =>
            {
                var instance = Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);
                instance.transform.SetParent(poolObject.transform);
                instance.Prefab = prefab.gameObject;
                return instance.gameObject;
            },
            actionOnGet: instance =>
            {
                instance.gameObject.SetActive(true);
                onGet?.Invoke(instance);
            },
            actionOnRelease: instance =>
            {
                instance.gameObject.SetActive(false);
                instance.transform.SetParent(poolObject.transform);
                onRelease?.Invoke(instance);
            },
            actionOnDestroy: Object.Destroy,
            defaultCapacity: initialSize,
            maxSize: MaxPoolSize);
        
        _pools.Add(prefab.gameObject, pool);
        _logger.Log($"Pool for prefab: {prefab.name} created");
        Warm(pool, initialSize);
        
        return pool;
    }

    private GameObject GetPoolObject(GameObject prefab)
    {
        var found =  _poolObjectsByPrefab.TryGetValue(prefab, out var poolObject);
        if (!found)
        {
            poolObject = new GameObject($"Pool_{prefab.name}");
            _poolObjectsByPrefab.Add(prefab, poolObject);
        }
        
        return poolObject;
    }

    public T Get<T>(T prefab) where T : Component, IPoolable
    {
        IObjectPool<GameObject> pool = null;
        if (!PoolExists(prefab.gameObject))
        {
            var c = prefab.GetComponent<T>();
            if (!c)
            {
                _logger.LogError($"{prefab.name} does not have a component of type {typeof(T).Name}");
                return null;
            }
            
            pool = CreatePool(c, 1);
        }
        
        pool ??= _pools[prefab.gameObject];
        var instance = pool?.Get();
        var component = instance?.GetComponent<T>();
        if (component)
        {
            return component;
        }
        
        _logger.LogError($"{instance} retrieved from pool: {prefab.name} does not contain component of type {typeof(T)}");
        return null;
    }

    public void Return<T>(T instance) where T : Component, IPoolable
    {
        var pool = GetPool(instance.Prefab);
        pool?.Release(instance.gameObject);
    }

    public void Warm(GameObject prefab, int size)
    {
        var pool = GetPool(prefab);
        Warm(pool, size);
    }

    private void Warm(IObjectPool<GameObject> pool, int size)
    {
        var instances = new List<GameObject>();
        for (var i = 0; i < size; i++)
        {
            var instance = pool?.Get();
            instances.Add(instance);
        }

        foreach (var instance in instances)
        {
            pool?.Release(instance);
        }
        
        _logger.Log($"pool warmed with {size} objects");
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
