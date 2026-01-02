using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BinhoGames.Core
{
    public static class SceneUtils
    {
        private static readonly Logger Logger = new(nameof(SceneUtils));
        
        public static SceneData CreateSceneData(Scene scene)
        {
            var gameObjects = scene.GetRootGameObjects();
            var lights = new HashSet<Light>();
            var lifecycle = default(SceneLifecycle);
            foreach (var gameObject in gameObjects)
            {
                var foundLights = gameObject.GetComponentsInChildren<Light>(true);
                if (foundLights is { Length: > 0 })
                {
                    foreach (var light in foundLights)
                    {
                        lights.Add(light);
                    }
                }
                
                var foundLifecycle = gameObject.GetComponentInChildren<SceneLifecycle>(true);
                if (foundLifecycle)
                {
                    if (lifecycle)
                    {
                        Logger.LogError($"More than 1 {nameof(SceneLifecycle)} found in scene: {scene.name}. Must be exactly 1");
                    }
                    
                    lifecycle = foundLifecycle;
                }
            }

            if (!lifecycle)
            {
                Logger.LogError($"No {nameof(SceneLifecycle)} found in scene: {scene}");
            }
                
            var sceneData = new SceneData
            {
                Name = scene.name,
                Scene = scene,
                Lifecycle = lifecycle,
                Lights = lights
            };
        
            return sceneData;
        }
    }
}
