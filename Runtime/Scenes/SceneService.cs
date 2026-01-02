using System.Collections;
using System.Collections.Generic;
using BinhoGames.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneService
{
    private readonly CoroutineRunner _coroutineRunner;
    private readonly Logger _logger;
    private readonly Stack<string> _sceneHistory = new();
    private readonly Dictionary<string, SceneData> _loadedSceneDataByName = new();
    private readonly List<string> _shownScenes = new();
    
    public SceneService() : this(Locator.Get<CoroutineRunner>())
    {
    }

    public SceneService(CoroutineRunner coroutineRunner)
    {
        _coroutineRunner = coroutineRunner;
        _logger = new Logger(nameof(SceneService));
        
        var initScene = SceneManager.GetSceneByName(SceneNames.InitScene);
        var initSceneData = SceneUtils.CreateSceneData(initScene);
        OnSceneLoaded(initSceneData);
    }
    
    public void Load(string sceneName)
    {
        _coroutineRunner.StartCoroutine(LoadCor(sceneName));
    }
    
    public IEnumerator LoadCor(string sceneName, bool skipValidation = false)
    {
        if (!skipValidation)
        {
            if (_loadedSceneDataByName.ContainsKey(sceneName))
            {
                _logger.LogError($"Scene: {sceneName} is already loaded");
                yield break;
            }
        }
        
        var load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        if (load == null)
        {
            _logger.LogError($"Failed to load scene {sceneName}");
            yield break;
        }
        
        _logger.Log($"Loading scene: {sceneName}");
        load.allowSceneActivation = false;
        yield return new WaitUntil(() => load.progress >= 0.9f);
        load.allowSceneActivation = true;
        var scene = SceneManager.GetSceneByName(sceneName);
        yield return new WaitUntil(() => scene.isLoaded);
        var sceneData = SceneUtils.CreateSceneData(scene);
        OnSceneLoaded(sceneData);
    }
    
    public void Unload(string sceneName)
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
        SceneManager.UnloadSceneAsync(sceneData.Scene);
        _loadedSceneDataByName.Remove(sceneName);
    }

    public void Show(string sceneName, bool addToHistory = true, bool autoLoad = true)
    {
        _coroutineRunner.StartCoroutine(ShowCor(sceneName, addToHistory, autoLoad));
    }

    public void Hide(string sceneName)
    {
        _coroutineRunner.StartCoroutine(HideCor(sceneName));
    }
    
    public IEnumerator ShowCor(string sceneName, bool addToHistory = true, bool autoLoad = true)
    {
        if (_shownScenes.Contains(sceneName))
        {
            _logger.LogWarning($"{sceneName} is already being shown");
            yield break;
        }
        
        var isLoaded = _loadedSceneDataByName.TryGetValue(sceneName, out var sceneData);
        if (!isLoaded)
        {
            if (!autoLoad)
            {
                _logger.LogError($"Cannot show scene: {sceneName} because it is not loaded");
                yield break;
            }

            yield return LoadCor(sceneName, skipValidation: true);
            if (!_loadedSceneDataByName.TryGetValue(sceneName, out var loadedSceneData))
            {
                _logger.LogError($"Cannot show scene: {sceneName} because the automatic load failed");
                yield break;
            }

            sceneData = loadedSceneData;
        }
        
        _logger.Log($"Showing scene: {sceneData.Name}");
        SceneManager.SetActiveScene(sceneData.Scene);
        _shownScenes.Add(sceneData.Name);
        if (sceneData.Lifecycle.Bootstrapper && !sceneData.Lifecycle.Bootstrapper.IsComplete)
        {
            yield return new WaitUntil(() => sceneData.Lifecycle.Bootstrapper.IsComplete);
        }
        
        yield return sceneData.Lifecycle.OnBeforeShow(sceneData);
        if (addToHistory)
        {
            var anotherSceneActive = _sceneHistory.TryPeek(out var activeSceneName);
            if (anotherSceneActive)
            {
                yield return HideCor(activeSceneName);
            }
            
            _sceneHistory.Push(sceneData.Name);
        }
        
        yield return sceneData.Lifecycle.OnShow(sceneData);
    }

    private IEnumerator HideCor(string sceneName)
    {
        if (!_shownScenes.Contains(sceneName))
        {
            _logger.LogWarning($"Cannot hide scene: {sceneName} because it is already not being shown");
            yield break;
        }
        
        var isLoaded = _loadedSceneDataByName.TryGetValue(sceneName, out var sceneData);
        if (!isLoaded)
        {
            _logger.LogError($"Cannot hide scene: {sceneName} because it is not loaded");
            yield break;
        }
        
        _logger.Log($"Hiding scene: {sceneName}");
        yield return sceneData.Lifecycle.OnHide(sceneData);
        yield return sceneData.Lifecycle.OnAfterHide(sceneData);
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
