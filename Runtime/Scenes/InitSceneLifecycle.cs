using System.Collections;
using BinhoGames.Core;

public class InitSceneLifecycle : SceneLifecycle
{
    public override IEnumerator OnBeforeShow(SceneData sceneData)
    {
        yield break;
    }

    public override IEnumerator OnShow(SceneData sceneData)
    {
        yield break;
    }

    public override IEnumerator OnHide(SceneData sceneData)
    {
        yield break;
    }

    public override IEnumerator OnAfterHide(SceneData sceneData)
    {
        yield break;
    }

    public override void HideImmediate(SceneData sceneData)
    {
    }
}
