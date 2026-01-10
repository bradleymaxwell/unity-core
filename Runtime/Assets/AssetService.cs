using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AssetService
{
    private readonly Dictionary<string, AsyncOperationHandle<Object>> _loadsByAsset = new();
    private readonly Logger _logger;

    public AssetService() : this(new Logger(nameof(AssetService)))
    {
    }

    public AssetService(Logger logger)
    {
        _logger = logger;
    }
    
    public async UniTask LoadAsync(AssetReference asset)
    {
        await LoadAsync(asset.RuntimeKey.ToString());
    }
    
    public async UniTask LoadAsync(string address)
    {
        var load = Addressables.LoadAssetAsync<Object>(address);
        await load.ToUniTask();
        if (load.Status != AsyncOperationStatus.Succeeded)
        {
            _logger.LogError($"Failed to load asset: {address}");
        }
        
        _loadsByAsset[address] = load;
    }

    public void Unload(string address)
    {
        if (_loadsByAsset.TryGetValue(address, out var load))
        {
            Addressables.Release(load);
            _loadsByAsset.Remove(address);
        }
        else
        {
            _logger.LogWarning($"Failed to unload asset: {address} as it is unloaded");
        }
    }

    public async UniTask<T> GetAsync<T>(AssetReference asset) where T : Object
    {
        var loadedAsset = await GetAsync<T>(asset.RuntimeKey.ToString());
        return loadedAsset;
    }
    
    public async UniTask<T> GetAsync<T>(string address) where T : Object
    {
        if (!IsLoaded(address))
        {
            await LoadAsync(address);
        }

        var loadedAsset = _loadsByAsset[address];
        if (loadedAsset.Result is T typedAsset)
        {
            return typedAsset;
        }
        
        _logger.LogError($"Loaded asset: {address} as it is not of type {typeof(T)}");
        return null;
    }

    public bool IsLoaded(string address)
    {
        return _loadsByAsset.ContainsKey(address);
    }
}
