using System;
using System.Collections.Generic;

public static class Locator
{
    private static readonly Dictionary<Type, object> Services = new();
    private static readonly Logger Logger = new(nameof(Locator));
    
    public static T Get<T>()
    {
        var type = typeof(T);
        var isRegistered = Services.TryGetValue(type, out var service);
        if (!isRegistered)
        {
            if (CanLazilyRegister(type))
            {
                var instance = (T)Activator.CreateInstance(type);
                Register(instance);
                return instance;
            }
            
            Logger.LogError($"Can't lazily register instance for type: {type}. must be explicitly registered first.");
            return default;
        }

        if (service is T typedService)
        {
            return typedService;
        }
    
        Logger.LogError($"Invalid service type {service.GetType()} registered for {type}");
        return default;
    }
    
    public static void Register<TContract, TImplementation>(Func<TImplementation> factory = null)
        where TImplementation : TContract, new()
    {
        var instance = factory != null ? factory() : new TImplementation();
        TryRegisterInstance<TContract>(instance);
    }
    
    public static void Register<T>()
        where T : new()
    {
        TryRegisterInstance(new T());
    }

    public static void Register<TContract>(TContract instance)
    {
        TryRegisterInstance(instance);
    }

    private static bool TryRegisterInstance<TContract>(TContract instance)
    {
        if (Services.ContainsKey(typeof(TContract)))
        {
            Logger.LogError($"The type {typeof(TContract)} is already registered");
            return false;
        }

        Services[typeof(TContract)] = instance;
        Logger.Log($"Registered {typeof(TContract)}");
        return true;
    }
    
    private static bool CanLazilyRegister(Type type)
    {
        if (type.IsAbstract || type.IsInterface)
        {
            return false;
        }

        var emptyCtor = type.GetConstructor(Type.EmptyTypes);
        return emptyCtor != null;
    }
}
