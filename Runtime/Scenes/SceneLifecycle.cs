using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BinhoGames.Core
{
    public abstract class SceneLifecycle : MonoBehaviour
    {
        public SceneBootstrapper Bootstrapper { get; private set; }
        
        private void Awake()
        {
            Bootstrapper = GetComponent<SceneBootstrapper>();
        }

        public abstract UniTask OnBeforeShowAsync(SceneData sceneData);
        public abstract UniTask OnShowAsync(SceneData sceneData);
        public abstract UniTask OnHideAsync(SceneData sceneData);
        public abstract UniTask OnAfterHideAsync(SceneData sceneData);
        public abstract void HideImmediate(SceneData sceneData);
    }
}
