using System.Collections;
using UnityEngine;

public abstract class SceneBootstrapper : MonoBehaviour
{
    public bool IsFinished { get; private set; }
    protected abstract BootstrapType BootstrapType { get; }
    
    private void Awake()
    {
        IsFinished = false;
        if (BootstrapType is BootstrapType.Sync or BootstrapType.Both)
        {
            OnBootstrap();
            if (BootstrapType is BootstrapType.Sync)
            {
                IsFinished = true;
                return;
            }
        }

        StartCoroutine(BootstrapCor());
    }

    protected virtual void OnBootstrap()
    {
    }
    
    protected virtual IEnumerator OnBootstrapCor()
    { 
        yield break;
    }

    private IEnumerator BootstrapCor()
    {
        yield return OnBootstrapCor();
        IsFinished = true;
    }
}

public enum BootstrapType
{
    Sync,
    Async,
    Both
}
