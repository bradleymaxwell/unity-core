using System.Collections;
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

        public abstract IEnumerator OnBeforeShow(SceneData sceneData);
        public abstract IEnumerator OnShow(SceneData sceneData);
        public abstract IEnumerator OnHide(SceneData sceneData);
        public abstract IEnumerator OnAfterHide(SceneData sceneData);
        public abstract void HideImmediate(SceneData sceneData);
    }
}
