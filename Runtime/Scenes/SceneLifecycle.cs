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

        public abstract IEnumerator OnBeforeShow();
        public abstract IEnumerator OnShow();
        public abstract IEnumerator OnHide();
        public abstract IEnumerator OnAfterHide();
        public abstract void HideImmediate();
    }
}
