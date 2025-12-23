using System;
using System.Collections;
using System.Collections.Generic;
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
    
    public IEnumerator LoadCor<T>(AssetReference asset, Action<T> onLoaded = null)
    {
        var load = Addressables.LoadAssetAsync<T>(asset);
        yield return load;
        if (load.Status != AsyncOperationStatus.Succeeded)
        {
            _logger.LogError($"Failed to load asset: {asset}");
            yield break;
        }
        
        onLoaded?.Invoke(load.Result);
        _loadsByAsset[asset] = load; 
    }

    public void Unload(AssetReference asset)
    {
        if (_loadsByAsset.TryGetValue(asset, out var load))
        {
            load.Release();
        }
        else
        {
            _logger.LogWarning($"Failed to unload asset: {asset} as it is unloaded");
        }
    }
}
