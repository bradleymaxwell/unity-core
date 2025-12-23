public class StartSceneBootstrapper : SceneBootstrapper
{
    protected override BootstrapType BootstrapType => BootstrapType.Sync;
    
    protected override void OnBootstrap()
    {
        base.OnBootstrap();
    }
}
