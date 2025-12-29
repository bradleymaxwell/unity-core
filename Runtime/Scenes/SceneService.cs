using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneService
{
    private readonly CoroutineRunner _coroutineRunner;
    private readonly Logger _logger;
    private readonly Stack<string> _sceneHistory = new();
    private readonly Dictionary<string, Scene> _loadedScenesByName = new();
    public string ActiveSceneName => _sceneHistory.Count > 0 ? _sceneHistory.Peek() : string.Empty;
    
    public SceneService() : this(Locator.Get<CoroutineRunner>())
    {
    }

    public SceneService(CoroutineRunner coroutineRunner)
    {
        _coroutineRunner = coroutineRunner;
        _logger = new Logger(nameof(SceneService));
    }
    
    public void Load(string sceneName, List<string> scenesToUnloadNames = null, bool setActive = true)
    {
        if (_loadedScenesByName.ContainsKey(sceneName))
        {
            _logger.LogWarning($"Scene: {sceneName} is already loaded");
            return;
        }
        
        _coroutineRunner.StartCoroutine(LoadCor(sceneName, scenesToUnloadNames, setActive));
    }

    public void Load(string sceneName, string sceneToUnload, bool setActive = true)
    {
        Load(sceneName, new List<string> { sceneToUnload }, setActive);
    }

    public void SetActive(string sceneName)
    {
        if (string.Equals(sceneName, ActiveSceneName))
        {
            _logger.LogWarning($"Scene: {sceneName} is already active");
            return;
        }
        
        var isLoaded = _loadedScenesByName.TryGetValue(sceneName, out var scene);
        if (!isLoaded)
        {
            _logger.LogError($"Cannot activate scene: {sceneName} because it is not loaded");
            return;
        }

        SetActive(scene);
    }

    public void Unload(string sceneName)
    {
        if (string.Equals(sceneName, ActiveSceneName))
        {
            _logger.LogError($"Scene: {sceneName} is the active scene, and only non-active scenes are allowed to be unloaded");
            return;
        }
        
        var isLoaded = _loadedScenesByName.TryGetValue(sceneName, out var scene);
        if (!isLoaded)
        {
            _logger.LogError($"Cannot unload scene: {sceneName} because it is not loaded");
            return;
        }

        SceneManager.UnloadSceneAsync(scene);
        _loadedScenesByName.Remove(sceneName);
    }

    private IEnumerator LoadCor(string sceneName, List<string> scenesToUnloadNames, bool setActive)
    {
        var load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        if (load == null)
        {
            _logger.LogError($"Failed to load scene {sceneName}");
            yield break;
        }

        load.allowSceneActivation = false;
        yield return new WaitUntil(() => load.progress >= 0.9f);
        load.allowSceneActivation = true;
        var scene = SceneManager.GetSceneByName(sceneName);
        yield return WaitUntilBootstrapperFinished(scene);
        _loadedScenesByName.Add(sceneName, scene);
        if (setActive)
        {
            SetActive(scene);
        }

        if (scenesToUnloadNames == null)
        {
            yield break;
        }

        foreach (var sceneNameToUnload in scenesToUnloadNames)
        {
            Unload(sceneNameToUnload);
        }
    }

    private void SetActive(Scene scene)
    {
        SceneManager.SetActiveScene(scene);
        _sceneHistory.Push(scene.name);
    }

    private IEnumerator WaitUntilBootstrapperFinished(Scene scene)
    {
        yield return new WaitUntil(() => scene.isLoaded);
        
        var gameObjects = scene.GetRootGameObjects();
        SceneBootstrapper bootstrapper = null;
        foreach (var gameObject in gameObjects)
        {
            var component = gameObject.GetComponentInChildren<SceneBootstrapper>();
            if (component)
            {
                bootstrapper = component;
                break;
            }
        }

        if (!bootstrapper)
        {
            _logger.LogWarning($"Could not find bootstrapper for {scene.name} so transitioning to {scene.name} immediately");
            yield break;
        }

        yield return new WaitUntil(() => bootstrapper.IsFinished);
    }
}
