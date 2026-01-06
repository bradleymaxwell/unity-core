using System.Collections;
using BinhoGames.Core;
using Cysharp.Threading.Tasks;

public class InitSceneLifecycle : SceneLifecycle
{
    public override UniTask OnBeforeShowAsync(SceneData sceneData)
    {
        return UniTask.CompletedTask;
    }

    public override UniTask OnShowAsync(SceneData sceneData)
    {
        return UniTask.CompletedTask;
    }

    public override UniTask OnHideAsync(SceneData sceneData)
    {
        return UniTask.CompletedTask;
    }

    public override UniTask OnAfterHideAsync(SceneData sceneData)
    {
        return UniTask.CompletedTask;
    }

    public override void HideImmediate(SceneData sceneData)
    {
    }
}
