using System.Collections;
using BinhoGames.Core;

public class InitSceneLifecycle : SceneLifecycle
{
    public override IEnumerator OnBeforeShow()
    {
        yield break;
    }

    public override IEnumerator OnShow()
    {
        yield break;
    }

    public override IEnumerator OnHide()
    {
        yield break;
    }

    public override IEnumerator OnAfterHide()
    {
        yield break;
    }

    public override void HideImmediate()
    {
    }
}
