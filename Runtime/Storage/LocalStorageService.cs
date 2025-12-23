using System;
using System.IO;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

public class LocalStorageService
{
    private readonly Logger _logger;

    public LocalStorageService()
    {
        _logger = new Logger(nameof(LocalStorageService));
    }

    public void Save<T>(T value, string relativeDirectory) where T : ISaveable
    {
        try
        {
            if (string.IsNullOrWhiteSpace(value.Id))
            {
                throw new ArgumentException("value needs to have an ID or it cannot be saved");
            }
            
            var path = GetJsonPath(relativeDirectory, value.Id);
            var directory = Path.GetDirectoryName(path);
            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new Exception($"Invalid relative directory: {relativeDirectory}");
            }
            
            var json = JsonConvert.SerializeObject(value, Formatting.Indented);
            Directory.CreateDirectory(directory);
            var tempPath = path + ".tmp";
            File.WriteAllText(tempPath, json);
            if (File.Exists(path))
            {
                File.Replace(tempPath, path, null);
            }
            else
            {
                File.Move(tempPath, path);
            }
            
            _logger.Log($"{value.Id} saved to {path}");
        }
        catch (Exception e)
        {
            _logger.LogError($"Failed to save {value}: {e}");
        }
    }

    public T Load<T>(string id, string relativeDirectory) where T : ISaveable
    {
        try
        {
            var path = GetJsonPath(relativeDirectory, id);
            if (!File.Exists(path))
            {
                _logger.LogError($"Saved data for: {id} not found in path: {path}");
                return default;
            }
        
            var data = File.ReadAllText(path);
            var value = JsonConvert.DeserializeObject<T>(data);
            return value;
        }
        catch (Exception e)
        {
            _logger.LogError($"Failed to load {typeof(T)}: {e.Message}");
            return default;
        }
    }

    private static string GetJsonPath(string relativeFolderPath, string id)
    {
        var folderPath = Path.Combine(Application.persistentDataPath, relativeFolderPath);
        var path = Path.Combine(folderPath, id + ".json");
        return path;
    }
}
