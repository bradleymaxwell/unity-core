using UnityEngine;
using UnityEngine.InputSystem;

public class InitSceneBootstrapper : SceneBootstrapper
{
    [SerializeField] private string nextSceneName;
    [SerializeField] private InputActionAsset playerInput;
    protected override BootstrapType BootstrapType => BootstrapType.Sync;
    
    protected override void OnBootstrap()
    {
        var coroutineRunnerGameObject = new GameObject(nameof(CoroutineRunner));
        var coroutineRunner = coroutineRunnerGameObject.AddComponent<CoroutineRunner>();
        DontDestroyOnLoad(coroutineRunnerGameObject);
        Locator.Register(coroutineRunner);
        
        var sceneService = new SceneService(coroutineRunner);
        Locator.Register(sceneService);
        
        var inputService = new InputService(playerInput);
        Locator.Register(inputService);
        
        nextSceneName = string.IsNullOrWhiteSpace(nextSceneName) ? SceneNames.StartScene : nextSceneName;
        sceneService.Load(nextSceneName, SceneNames.InitScene);
    }
}
