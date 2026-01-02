using UnityEngine.SceneManagement;

namespace BinhoGames.Core
{
    public class SceneData
    {
        public string Name { get; set; }
        public Scene Scene { get; set; }
        public SceneLifecycle Lifecycle { get; set; }
    }
}
