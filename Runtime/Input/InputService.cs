using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputService
{
    private readonly InputActionAsset _asset;
    private readonly HashSet<InputActionMap> _activeMaps = new();
    private readonly Logger _logger;
    
    public InputService(InputActionAsset inputActionAsset)
    {
        _logger = new Logger(nameof(InputService));
        if (inputActionAsset == null)
        {
            _logger.LogError($"{nameof(inputActionAsset)} cannot be null!");
            return;
        }
        
        _asset = inputActionAsset;
        Toggle(false);
        Activate(_asset.actionMaps[0]);
    }

    public void Toggle(bool toggle)
    {
        if (toggle)
        {
            foreach (var map in _activeMaps)
            {
                Activate(map);
            }
        }
        else
        {
            _asset.Disable();
        }
    }

    public InputAction GetAction(string mapName, string actionName)
    {
        var map = GetMap(mapName);
        var action = map.FindAction(actionName, true);
        return action;
    }

    public InputActionMap GetMap(string mapName)
    {
        var map = _asset.FindActionMap(mapName, true);
        return map;
    }
    
    public void Activate(string mapName)
    {
        var map = GetMap(mapName);
        Activate(map);
    }

    public void Deactivate(string mapName)
    {
        var map = GetMap(mapName);
        Deactivate(map);
    }
    
    private void Activate(InputActionMap map)
    {
        if (!_activeMaps.Contains(map))
        {
            map.Enable();
            _activeMaps.Add(map);
        }
    }

    private void Deactivate(InputActionMap map)
    {
        if (_activeMaps.Contains(map))
        {
            map.Disable();
            _activeMaps.Remove(map);
        }
    }
}
