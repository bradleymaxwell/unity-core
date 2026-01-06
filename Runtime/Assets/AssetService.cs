using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AssetService
{
    private readonly Dictionary<AssetReference, AsyncOperationHandle> _loadsByAsset = new();
    private readonly Logger _logger;

    public AssetService() : this(new Logger(nameof(AssetService)))
    {
    }

    public AssetService(Logger logger)
    {
        _logger = logger;
    }
    
    public async UniTask<T> LoadAsync<T>(AssetReference asset, Action<T> onLoaded = null)
    {
        var load = Addressables.LoadAssetAsync<T>(asset);
        await load.ToUniTask();
        if (load.Status != AsyncOperationStatus.Succeeded)
        {
            _logger.LogError($"Failed to load asset: {asset}");
            return default;
        }
        
        onLoaded?.Invoke(load.Result);
        _loadsByAsset[asset] = load;
        return load.Result;
    }

    public void Unload(AssetReference asset)
    {
        if (_loadsByAsset.TryGetValue(asset, out var load))
        {
            load.Release();
            _loadsByAsset.Remove(asset);
        }
        else
        {
            _logger.LogWarning($"Failed to unload asset: {asset} as it is unloaded");
        }
    }

    public bool IsLoaded(AssetReference asset)
    {
        return _loadsByAsset.ContainsKey(asset);
    }
}
