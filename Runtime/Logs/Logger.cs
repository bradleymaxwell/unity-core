using System.Collections.Generic;
using UnityEngine;

public class Logger
{
    private const bool AllCategoriesEnabled = true;
    private static readonly HashSet<string> EnabledCategories = new();
    private readonly string _name;
    private readonly bool _isEnabled;
    
    public Logger(string name, string category = null)
    {
        _name = name;
        _isEnabled = AllCategoriesEnabled || EnabledCategories.Contains(category);
    }

    public void Log(string message)
    {
        #if LOGS_ENABLED
        if (_isEnabled)
        {
            Debug.Log(_name + ": " + message);
        }
        #endif
    }

    public void LogError(string message)
    {
        Debug.LogError(_name + ": " + message);
    }

    public void LogWarning(string message)
    {
        #if WARNINGS_ENABLED
        Debug.LogWarning(_name + ": " + message);
        #endif
    }
}
