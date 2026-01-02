using System.Collections;
using UnityEngine;

public abstract class SceneBootstrapper : MonoBehaviour
{
    public bool IsComplete { get; private set; }
    protected abstract BootstrapType BootstrapType { get; }
    
    private void Awake()
    {
        IsComplete = false;
        if (BootstrapType is BootstrapType.Sync or BootstrapType.Both)
        {
            OnBootstrap();
            if (BootstrapType is BootstrapType.Sync)
            {
                IsComplete = true;
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
        IsComplete = true;
    }
}

public enum BootstrapType
{
    Sync,
    Async,
    Both
}
