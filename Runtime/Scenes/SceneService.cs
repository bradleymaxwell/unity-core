using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneService
{
    private readonly CoroutineRunner _coroutineRunner;
    private readonly Logger _logger;
    private readonly Stack<string> _sceneHistory = new();
    
    public SceneService() : this(Locator.Get<CoroutineRunner>())
    {
    }

    public SceneService(CoroutineRunner coroutineRunner)
    {
        _coroutineRunner = coroutineRunner;
        _logger = new Logger(nameof(SceneService));
    }
    
    public void Load(string sceneName, List<string> scenesToUnloadNames = null)
    {
        _coroutineRunner.StartCoroutine(LoadCor(sceneName, scenesToUnloadNames));
    }

    public void Load(string sceneName, string sceneToUnload)
    {
        Load(sceneName, new List<string> { sceneToUnload });
    }

    private IEnumerator LoadCor(string sceneName, List<string> scenesToUnloadNames)
    {
        var load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        if (load == null)
        {
            _logger.LogError($"Failed to load scene {sceneName}");
            yield break;
        }
        
        load.allowSceneActivation = false;
        yield return new WaitUntil(() => load.progress >= 0.9f);
        yield return WaitUntilBootstrapperFinished(sceneName);
        load.allowSceneActivation = true;
        _sceneHistory.Push(sceneName);

        if (scenesToUnloadNames == null)
        {
            yield break;
        }
        
        foreach (var sceneNameToUnload in scenesToUnloadNames)
        {
            SceneManager.UnloadSceneAsync(sceneNameToUnload);
        }
    }

    private IEnumerator WaitUntilBootstrapperFinished(string sceneName)
    {
        var scene = SceneManager.GetSceneByName(sceneName);
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
            _logger.LogWarning($"Could not find bootstrapper for {sceneName} so transitioning to {sceneName} immediately");
            yield break;
        }

        yield return new WaitUntil(() => bootstrapper.IsFinished);
    }
}
