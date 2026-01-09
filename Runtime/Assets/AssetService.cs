using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AssetService
{
    private readonly Dictionary<AssetReference, AsyncOperationHandle<UnityEngine.Object>> _loadsByAsset = new();
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
        var load = Addressables.LoadAssetAsync<Object>(asset);
        await load.ToUniTask();
        if (load.Status != AsyncOperationStatus.Succeeded)
        {
            _logger.LogError($"Failed to load asset: {asset}");
        }
        
        _loadsByAsset[asset] = load;
    }

    public void Unload(AssetReference asset)
    {
        if (_loadsByAsset.TryGetValue(asset, out var load))
        {
            Addressables.Release(load);
            _loadsByAsset.Remove(asset);
        }
        else
        {
            _logger.LogWarning($"Failed to unload asset: {asset} as it is unloaded");
        }
    }

    public async UniTask<T> GetAsync<T>(AssetReference asset) where T : Object
    {
        if (!IsLoaded(asset))
        {
            await LoadAsync(asset);
        }

        var loadedAsset = _loadsByAsset[asset];
        if (loadedAsset.Result is T typedAsset)
        {
            return typedAsset;
        }
        
        _logger.LogError($"Loaded asset: {asset} as it is not of type {typeof(T)}");
        return null;
    }

    public bool IsLoaded(AssetReference asset)
    {
        return _loadsByAsset.ContainsKey(asset);
    }
}
