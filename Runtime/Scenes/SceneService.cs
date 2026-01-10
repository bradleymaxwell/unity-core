using System.Collections;
using System.Collections.Generic;
using BinhoGames.Core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneService
{
    private readonly Logger _logger;
    private readonly Stack<string> _sceneHistory = new();
    private readonly Dictionary<string, SceneData> _loadedSceneDataByName = new();
    private readonly List<string> _shownScenes = new();
    
    public SceneService()
    {
        _logger = new Logger(nameof(SceneService));
        
        var initScene = SceneManager.GetSceneByName(SceneNames.InitScene);
        var initSceneData = SceneUtils.CreateSceneData(initScene);
        OnSceneLoaded(initSceneData);
    }

    public async UniTask LoadAsync(string sceneName, bool skipValidation = false)
    {
        if (!skipValidation)
        {
            if (_loadedSceneDataByName.ContainsKey(sceneName))
            {
                _logger.LogError($"Scene: {sceneName} is already loaded");
                return;
            }
        }
        
        var load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        if (load == null)
        {
            _logger.LogError($"Failed to load scene {sceneName}");
            return;
        }
        
        _logger.Log($"Loading scene: {sceneName}");
        await load.ToUniTask();
        var scene = SceneManager.GetSceneByName(sceneName);
        var sceneData = SceneUtils.CreateSceneData(scene);
        OnSceneLoaded(sceneData);
    }
    
    public async UniTask UnloadAsync(string sceneName)
    {
        if (_shownScenes.Contains(sceneName))
        {
            _logger.LogError($"Cannot unload scene {sceneName} because it is being shown and needs to be hidden before unloaded");
            return;
        }
        
        var isLoaded = _loadedSceneDataByName.TryGetValue(sceneName, out var sceneData);
        if (!isLoaded)
        {
            _logger.LogWarning($"Cannot unload scene: {sceneName} because it is already unloaded");
            return;
        }
        
        _logger.Log($"Unloading scene: {sceneName}");
        var unload = SceneManager.UnloadSceneAsync(sceneData.Scene);
        await unload.ToUniTask();
        _loadedSceneDataByName.Remove(sceneName);
    }
    
    public async UniTask ShowAsync(string sceneName, bool addToHistory = true, bool autoLoad = true)
    {
        if (_shownScenes.Contains(sceneName))
        {
            _logger.LogWarning($"{sceneName} is already being shown");
            return;
        }
        
        var isLoaded = _loadedSceneDataByName.TryGetValue(sceneName, out var sceneData);
        if (!isLoaded)
        {
            if (!autoLoad)
            {
                _logger.LogError($"Cannot show scene: {sceneName} because it is not loaded");
                return;
            }

            await LoadAsync(sceneName, skipValidation: true);
            if (!_loadedSceneDataByName.TryGetValue(sceneName, out var loadedSceneData))
            {
                _logger.LogError($"Cannot show scene: {sceneName} because the automatic load failed");
                return;
            }

            sceneData = loadedSceneData;
        }
        
        _logger.Log($"Showing scene: {sceneData.Name}");
        SceneManager.SetActiveScene(sceneData.Scene);
        _shownScenes.Add(sceneData.Name);
        if (sceneData.Lifecycle.Bootstrapper && !sceneData.Lifecycle.Bootstrapper.IsComplete)
        {
            UniTask.WaitUntil(() => sceneData.Lifecycle.Bootstrapper.IsComplete);
        }
        
        await sceneData.Lifecycle.OnBeforeShowAsync(sceneData);
        if (addToHistory)
        {
            var anotherSceneActive = _sceneHistory.TryPeek(out var activeSceneName);
            if (anotherSceneActive)
            {
                await HideAsync(activeSceneName);
            }
            
            _sceneHistory.Push(sceneData.Name);
        }
        
        await sceneData.Lifecycle.OnShowAsync(sceneData);
    }

    public async UniTask HideAsync(string sceneName)
    {
        if (!_shownScenes.Contains(sceneName))
        {
            _logger.LogWarning($"Cannot hide scene: {sceneName} because it is already not being shown");
            return;
        }
        
        var isLoaded = _loadedSceneDataByName.TryGetValue(sceneName, out var sceneData);
        if (!isLoaded)
        {
            _logger.LogError($"Cannot hide scene: {sceneName} because it is not loaded");
            return;
        }
        
        _logger.Log($"Hiding scene: {sceneName}");
        await sceneData.Lifecycle.OnHideAsync(sceneData);
        await sceneData.Lifecycle.OnAfterHideAsync(sceneData);
        _shownScenes.Remove(sceneData.Name);
        var activeSceneData = GetActiveSceneData();
        if (sceneData.Name.Equals(activeSceneData.Name))
        {
            _sceneHistory.Pop();
            var newActiveSceneData = GetActiveSceneData();
            if (newActiveSceneData != null)
            {
                SceneManager.SetActiveScene(newActiveSceneData.Scene);
            }
        }
    }

    private void OnSceneLoaded(SceneData sceneData)
    {
        _loadedSceneDataByName.Add(sceneData.Name, sceneData);
        sceneData.Lifecycle.HideImmediate(sceneData);
        sceneData.Lifecycle.Initialize();
    }

    private SceneData GetActiveSceneData()
    {
        var hasActiveScene = _sceneHistory.TryPeek(out var activeSceneName);
        if (!hasActiveScene)
        {
            return null;
        }
        
        var isLoaded = _loadedSceneDataByName.TryGetValue(activeSceneName, out var sceneData);
        if (!isLoaded)
        {
            _logger.LogError($"scene: {activeSceneName} is tracked as the active scene but it's scene data is missing, indicating its not loaded");
            return null;
        }
        
        return sceneData;
    }
}
