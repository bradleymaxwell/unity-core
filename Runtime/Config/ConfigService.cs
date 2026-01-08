using System;
using System.Collections.Generic;

namespace BinhoGames.Core
{
    public class ConfigService
    {
        private readonly Dictionary<Type, ConfigContainer> _containers = new();
        private readonly Logger _logger;
        
        public ConfigService()
        {
            _logger = new Logger(nameof(ConfigService));
        }
        
        public T Get<T>() where T : ConfigContainer
        {
            if (_containers.TryGetValue(typeof(T), out var container))
            {
                return (T)container;
            }
            
            _logger.LogError($"{typeof(T)} config not found. Ensure the container is being registered beforehand.");
            return null;
        }

        public void Register(ConfigContainer config)
        {
            var type = config.GetType();
            if (!_containers.TryAdd(type, config))
            {
                _logger.LogError($"{type} is already registered.");
                return;
            }

            _logger.Log($"{config} registered for config type: {type}");
        }
    }
}
