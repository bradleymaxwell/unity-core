using System;
using System.Collections.Generic;

public class DomainEventService
{
    private readonly Dictionary<Type, List<IDomainEventListener>> _listeners = new();
    private readonly Logger _logger;

    public DomainEventService()
    {
        _logger = new Logger(nameof(DomainEventService));
    }
    
    public void Register<T>(IDomainEventListener<T> listener) where T : DomainEvent
    {
        if (IsRegistered(listener, out var registeredListeners))
        {
            _logger.LogWarning($"{listener} is already registered");
            return;
        }
        
        if (registeredListeners == null)
        {
            registeredListeners = new List<IDomainEventListener>();
            _listeners[typeof(T)] = registeredListeners;
        }
        
        registeredListeners.Add(listener);
    }

    public void Unregister<T>(IDomainEventListener<T> listener) where T : DomainEvent
    {
        if (!IsRegistered(listener, out var registeredListeners))
        {
            _logger.LogWarning($"{listener} is not registered");
            return;
        }

        registeredListeners?.Remove(listener);
        if (registeredListeners?.Count == 0)
        {
            _listeners.Remove(typeof(T));
        }
    }

    public void Raise<T>(T domainEvent) where T : DomainEvent
    {
        var hasListeners = _listeners.TryGetValue(typeof(T), out var registeredListeners);
        if (!hasListeners)
        {
            return;
        }
        
        var listeners = registeredListeners.ToArray();
        foreach (var listener in listeners)
        {
            try
            {
                if (listener is IDomainEventListener<T> typedListener)
                {
                    typedListener.OnEventRaised(domainEvent);
                }
                else
                {
                    throw new Exception($"Registered listener {listener} does not implement {nameof(IDomainEventListener)}<{nameof(T)}> and should not have been registered");
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Error encountered when raising {domainEvent}: {e}");
            }
        }
    }

    public int GetListenerCount<T>() where T : DomainEvent
    {
        var hasListeners = _listeners.TryGetValue(typeof(T), out var registeredListeners);
        return hasListeners ? registeredListeners.Count : 0;
    }
    
    public bool IsRegistered<T>(IDomainEventListener<T> listener) where T : DomainEvent
    {
        return IsRegistered(listener, out _);
    }
    
    private bool IsRegistered<T>(IDomainEventListener<T> listener, out List<IDomainEventListener> registeredListeners) where T : DomainEvent
    {
        var hasListeners = _listeners.TryGetValue(typeof(T), out var listeners);
        if (!hasListeners)
        {
            registeredListeners = null;
            return false;
        }
        
        registeredListeners = listeners;
        return listeners.Contains(listener);
    }
}
