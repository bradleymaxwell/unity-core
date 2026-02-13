using UnityEngine;

public interface IPoolable
{
    void Reset();
    GameObject Prefab { get; set; } 
}
